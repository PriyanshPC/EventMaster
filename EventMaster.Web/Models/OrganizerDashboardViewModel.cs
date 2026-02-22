namespace EventMaster.Web.Models;

public class OrganizerDashboardViewModel
{
    public string ActiveTab { get; set; } = "events";
    public MeSettingsViewModel Settings { get; set; } = new();
    public List<OrganizerEventListItemViewModel> Events { get; set; } = new();
    public OrganizerCreateEventFormViewModel AddEventForm { get; set; } = new();
    public List<OrganizerPendingReviewViewModel> PendingReviews { get; set; } = new();
}

public class OrganizerEventListItemViewModel
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<string> OccurrenceStatuses { get; set; } = new();
}

public class OrganizerCreateEventFormViewModel
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public List<string> Categories { get; set; } = new();
    public List<VenueOptionViewModel> Venues { get; set; } = new();
    public List<OrganizerOccurrenceRowViewModel> Occurrences { get; set; } = new() { new() };
}

public class VenueOptionViewModel
{
    public int VenueId { get; set; }
    public string Name { get; set; } = "";
}

public class OrganizerOccurrenceRowViewModel
{
    public DateOnly? Date { get; set; }
    public string Time { get; set; } = "";
    public int? VenueId { get; set; }
    public decimal? Price { get; set; }
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
