using EventMaster.Web.Services;
using EventMaster.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventMaster.Controllers;

public class HomeController : Controller
{
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

        // Your requirement: landing shows upcoming events grouped by categories.
        // Each “event” card should represent ONE upcoming occurrence (usually the soonest occurrence).
        var rows = upcoming
        .GroupBy(e => e.Category ?? "Other")
        .OrderBy(g => g.Key)
        .Select(g =>
        {
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
            var apiBase = (_config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');
            return new LandingEventCardViewModel
            {
                EventId = e.EventId,
                OccurrenceId = occ.OccurrenceId,
                Title = e.Name,
                SubTitle = subtitle,

                // Use the public image endpoint from your API
                ImageUrl = $"{apiBase}/api/events/{e.EventId}/image"
            };
        })
        .Where(x => x != null)
        .Cast<LandingEventCardViewModel>
            ()
            .Take(10)
            .ToList();

            return new LandingCategoryRowViewModel
            {
                CategoryKey = Slug(g.Key),
                CategoryName = g.Key,
                Events = cards
            };
        })
            .Where(r => r.Events.Count > 0)
            .ToList();

        return View(new LandingPageViewModel { Rows = rows });
    }

    private static string Slug(string s)
    => new string(s.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray())
    .Replace("--", "-")
    .Trim('-');
}
