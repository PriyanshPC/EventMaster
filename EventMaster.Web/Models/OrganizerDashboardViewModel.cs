namespace EventMaster.Web.Models;

public class OrganizerDashboardViewModel
{
    public string ActiveTab { get; set; } = "events";
    public MeSettingsViewModel Settings { get; set; } = new();
    public List<OrganizerEventCardViewModel> EventCards { get; set; } = new();
    public OrganizerCreateEventFormViewModel AddEventForm { get; set; } = new();
    public List<OrganizerPendingReviewViewModel> PendingReviews { get; set; } = new();
}

public class OrganizerEventCardViewModel
{
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string SubTitle { get; set; } = "";
    public string VenueLine { get; set; } = "";
    public string Status { get; set; } = "";
    public string ImageUrl { get; set; } = "";
}

public class OrganizerCreateEventFormViewModel
{
    public List<string> Categories { get; set; } = new();
    public List<VenueOptionViewModel> Venues { get; set; } = new();
}

public class VenueOptionViewModel
{
    public int VenueId { get; set; }
    public string Name { get; set; } = "";
}

public class OrganizerPendingReviewViewModel
{
    public int ReviewId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
