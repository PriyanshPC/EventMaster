namespace EventMaster.Web.Models;

public class AllEventsViewModel
{
    public string PageTitle { get; set; } = "All Events";

    // Filters (empty by default so "See all" truly shows no filters)
    public string Location { get; set; } = "";     // "Toronto, ON"
    public DateOnly? DateFrom { get; set; } = null; // show events on/after date
    public string? Category { get; set; } = null;  // e.g. "Music"
    public string? Q { get; set; } = null;         // event name only

    // Dropdown/options sources
    public List<string> Locations { get; set; } = new();
    public List<CategoryOptionVm> Categories { get; set; } = new();

    // Results
    public List<EventOccurrenceCardVm> Events { get; set; } = new();
}

public class CategoryOptionVm
{
    public string Value { get; set; } = ""; // querystring value
    public string Name { get; set; } = "";  // display name
}

public class EventOccurrenceCardVm
{
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }

    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string SubTitle { get; set; } = "";   // "Mar 2 • 07:30 PM • Toronto, ON"
    public string VenueLine { get; set; } = "";  // "Venue Name"
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = "";

    // for sorting/filtering
    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
}