namespace EventMaster.Web.Models;

/// <summary>
/// ViewModel for the organizer dashboard page, which includes the organizer's profile settings, a list of their events with relevant details, a form for creating new events, and a list of pending reviews from customers. Contains properties for the active tab (events, create event, or reviews), organizer profile information, a list of event cards with details for each event occurrence, a form ViewModel for creating new events with dropdown options for categories and venues, and a list of pending reviews with details about the review and the customer who submitted it. This ViewModel is populated by calling the Auth API to fetch the organizer's profile information, the Events API to fetch their events, and the Reviews API to fetch pending reviews for their events. The ViewModel is used to render the organizer dashboard page with all the necessary information and controls for managing their events and reviews.
/// </summary>
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
