namespace EventMaster.Web.Models;
/// <summary>
/// ViewModel for the landing page, which displays a selection of upcoming events organized by category. Contains a list of category rows, where each row has a category name and a list of event cards with basic information (event name, date/time, venue, image) to entice users to click through to the event details page. This ViewModel is populated by calling the Events API to fetch a curated selection of upcoming events for each category to feature on the landing page.
/// </summary>
public class LandingPageViewModel
{
    public List<LandingCategoryRowViewModel> Rows { get; set; } = new();
}

public class LandingCategoryRowViewModel
{
    public string CategoryKey { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public List<LandingEventCardViewModel> Events { get; set; } = new();
}

public class LandingEventCardViewModel
{
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }           // used in /Events/Details/{id}
    public string Title { get; set; } = "";
    public string SubTitle { get; set; } = "";
    public string ImageUrl { get; set; } = "";
}