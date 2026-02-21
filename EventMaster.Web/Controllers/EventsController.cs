using EventMaster.Web.Models;
using EventMaster.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventmMaster.Web.Controllers;

public class EventsController : Controller
{
    private readonly EventsApiClient _eventsApi;
    private readonly ReviewsApiClient _reviewsApi;
    private readonly IConfiguration _config;

    public EventsController(EventsApiClient eventsApi, ReviewsApiClient reviewsApi, IConfiguration config)
    {
        _eventsApi = eventsApi;
        _reviewsApi = reviewsApi;
        _config = config;
    }

    // GET /Events
    // GET /Events?category=Music
    // GET /Events?location=Toronto,%20ON&dateFrom=2026-02-22&category=Music&q=Drake
    [HttpGet]
    public async Task<IActionResult> Index(string? location, DateOnly? dateFrom, string? category, string? q)
    {
        // Entry mode detection
        var hasAny = !string.IsNullOrWhiteSpace(location)
                     || dateFrom is not null
                     || !string.IsNullOrWhiteSpace(category)
                     || !string.IsNullOrWhiteSpace(q);

        var categoryOnly = !string.IsNullOrWhiteSpace(category)
                           && string.IsNullOrWhiteSpace(location)
                           && dateFrom is null
                           && string.IsNullOrWhiteSpace(q);

        var vm = new AllEventsViewModel();

        if (!hasAny)
        {
            // A) From "See all events": no filters
            vm.PageTitle = "All Events";
        }
        else if (categoryOnly)
        {
            // B) From landing card: category only; rest empty
            vm.Category = category!.Trim();
            vm.PageTitle = $"{vm.Category} Events";
        }
        else
        {
            // User-driven filtering (form usage)
            vm.Location = location?.Trim() ?? "";
            vm.DateFrom = dateFrom;
            vm.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            vm.Q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            vm.PageTitle = string.IsNullOrWhiteSpace(vm.Category) ? "All Events" : $"{vm.Category} Events";
        }

        // Always fetch UNFILTERED upcoming so dropdowns (categories/locations) are complete.
        var upcomingAll = await _eventsApi.GetUpcomingAsync(city: null, q: null, category: null);

        // Build Categories from unfiltered dataset
        var categoryNames = upcomingAll
            .Select(e => string.IsNullOrWhiteSpace(e.Category) ? "Other" : e.Category.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        vm.Categories = new List<CategoryOptionVm> { new() { Value = "", Name = "All Categories" } };
        vm.Categories.AddRange(categoryNames.Select(c => new CategoryOptionVm { Value = c, Name = c }));

        // Build Locations (City, Province) from unfiltered dataset
        vm.Locations = upcomingAll
            .SelectMany(e => e.Occurrences)
            .Select(o => $"{o.City}, {o.Province}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        // Now build cards and apply filters in-memory
        var apiBase = (_config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');

        var cards = new List<EventOccurrenceCardVm>();

        foreach (var e in upcomingAll)
        {
            var cat = string.IsNullOrWhiteSpace(e.Category) ? "Other" : e.Category.Trim();

            foreach (var occ in e.Occurrences.OrderBy(o => o.Date).ThenBy(o => o.Time))
            {
                // DateFrom filter: show events ON/AFTER dateFrom
                if (vm.DateFrom is not null && occ.Date < vm.DateFrom.Value)
                    continue;

                // Location filter: must match exactly "City, Province"
                if (!string.IsNullOrWhiteSpace(vm.Location))
                {
                    var occLoc = $"{occ.City}, {occ.Province}";
                    if (!occLoc.Equals(vm.Location, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                // Category filter
                if (!string.IsNullOrWhiteSpace(vm.Category)
                    && !cat.Equals(vm.Category, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Search filter: event name only (requires search button)
                if (!string.IsNullOrWhiteSpace(vm.Q)
                    && !e.Name.Contains(vm.Q, StringComparison.OrdinalIgnoreCase))
                    continue;

                // 12-hr time formatting (AM/PM)
                var time12hr = DateTime.Today.Add(occ.Time).ToString("hh:mm tt");
                var subtitle = $"{occ.Date:MMM d} • {time12hr} • {occ.City}, {occ.Province}";

                cards.Add(new EventOccurrenceCardVm
                {
                    EventId = e.EventId,
                    OccurrenceId = occ.OccurrenceId,
                    Category = cat,
                    Title = e.Name,
                    SubTitle = subtitle,
                    VenueLine = occ.VenueName,
                    Price = occ.Price,
                    Date = occ.Date,
                    Time = occ.Time,
                    ImageUrl = $"{apiBase}/api/events/{e.EventId}/image"
                });
            }
        }

        // Sort upcoming: soonest first
        vm.Events = cards
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Time)
            .ThenBy(x => x.Title)
            .ToList();

        return View(vm);
    }

    // GET /Events/Details?eventId=123&occurrenceId=456
    [HttpGet]
    public async Task<IActionResult> Details(int eventId, int occurrenceId)
    {
        var details = await _eventsApi.GetOccurrenceDetailsAsync(eventId, occurrenceId);
        if (details is null) return NotFound();

        var reviews = await _reviewsApi.GetForEventAsync(eventId);

        // 12-hr format
        var time12hr = DateTime.Today.Add(details.Time).ToString("hh:mm tt");
        var headerWhen = $"{details.Date:MMM d, yyyy} • {time12hr}";
        var headerWhere = $"{details.VenueName} • {details.City}, {details.Province}";

        var apiBase = (_config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');
        var imageUrl = $"{apiBase}/api/events/{eventId}/image";

        var vm = new EventDetailsViewModel
        {
            EventId = eventId,
            OccurrenceId = occurrenceId,
            Details = details,
            HeaderWhen = headerWhen,
            HeaderWhere = headerWhere,
            ImageUrl = imageUrl,
            Reviews = reviews,
            ShowAddReviewButton = User.Identity?.IsAuthenticated == true && User.IsInRole("CUSTOMER")
        };

        return View(vm);
    }
}