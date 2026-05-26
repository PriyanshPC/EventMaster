using EventMaster.Web.Services;
using EventMaster.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventMaster.Controllers;

public class HomeController : Controller
{/// <summary>
/// Displays the landing page with upcoming events grouped by category. Integrates with the Events API to fetch upcoming events and constructs view models for rendering. Normalizes category names for grouping and creates URL-friendly slugs for category links. The landing page provides a snapshot of popular events, encouraging users to explore and book tickets.
/// The Index action retrieves upcoming events, groups them by normalized category, and builds a view model
/// that includes event details formatted for display (date/time, location) and image URLs. It ensures that categories are displayed in a user-friendly way while maintaining consistent grouping logic.
/// The NormalizeCategory method standardizes category names for grouping, treating null/empty as "other
/// and ignoring case and whitespace. The Slug method creates URL-friendly slugs for category links by replacing non-alphanumeric characters with hyphens.
/// This controller relies on the Events API to fetch event data and uses configuration settings to construct image URLs. The resulting view model is passed to the Razor view for rendering the landing page UI.
/// </summary>
    private readonly EventsApiClient _eventsApi;
    private readonly IConfiguration _config;

    public HomeController(EventsApiClient eventsApi, IConfiguration config)
    {
        _eventsApi = eventsApi;
        _config = config;
    }

    public async Task<IActionResult>
    Index()
    {
        // For “Popular near you” you can later pass city from user preference; for now leave null.
        var upcoming = await _eventsApi.GetUpcomingAsync();

        // Landing shows upcoming events grouped by category.
        // Grouping is normalized so categories like "Comedy" and "comedy" do not create duplicate rows.
        var rows = upcoming
            .GroupBy(e => NormalizeCategory(e.Category))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var displayCategory = g
                    .Select(e => string.IsNullOrWhiteSpace(e.Category) ? "Other" : e.Category.Trim())
                    .FirstOrDefault() ?? "Other";

                var cards = g
                    .Select(e =>
                    {
                        var occ = e.Occurrences
                            .OrderBy(o => o.Date)
                            .ThenBy(o => o.Time)
                            .FirstOrDefault();

                        if (occ == null) return null;

                        var time12hr = DateTime.Today.Add(occ.Time).ToString("h:mm tt");
                        var when = $"{occ.Date:MMM d} • {time12hr}";
                        var where = $"{occ.City}, {occ.Province}";
                        var subtitle = $"{when} • {where}";
                        var apiBase = (_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');

                        return new LandingEventCardCandidate
                        {
                            Card = new LandingEventCardViewModel
                            {
                                EventId = e.EventId,
                                OccurrenceId = occ.OccurrenceId,
                                Title = e.Name,
                                SubTitle = subtitle,
                                ImageUrl = $"{apiBase}/api/events/{e.EventId}/image"
                            },
                            OccurrenceDate = occ.Date,
                            OccurrenceTime = occ.Time
                        };
                    })
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .OrderBy(x => x.OccurrenceDate)
                    .ThenBy(x => x.OccurrenceTime)
                    .Take(8)
                    .Select(x => x.Card)
                    .ToList();

                return new LandingCategoryRowViewModel
                {
                    CategoryKey = Slug(displayCategory),
                    CategoryName = displayCategory,
                    Events = cards
                };
            })
            .Where(r => r.Events.Count > 0)
            .ToList();

        return View(new LandingPageViewModel { Rows = rows });
    }

    private static string NormalizeCategory(string? category)
        => string.IsNullOrWhiteSpace(category) ? "other" : category.Trim().ToLowerInvariant();

    private static string Slug(string s)
        => new string(s.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray())
            .Replace("--", "-")
            .Trim('-');

    private sealed class LandingEventCardCandidate
    {
        public LandingEventCardViewModel Card { get; set; } = new();
        public DateOnly OccurrenceDate { get; set; }
        public TimeSpan OccurrenceTime { get; set; }
    }
}
