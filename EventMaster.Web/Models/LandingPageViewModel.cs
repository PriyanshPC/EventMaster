namespace EventMaster.Web.Models;

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