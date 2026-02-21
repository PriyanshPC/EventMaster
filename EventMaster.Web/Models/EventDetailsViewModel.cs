using EventMaster.Web.Services.ApiDtos;

namespace EventMaster.Web.Models;

public class EventDetailsViewModel
{
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }

    public EventOccurrenceDetailsResponse Details { get; set; } = new();

    public string HeaderWhen { get; set; } = "";   // "Mar 2, 2026 • 07:30 PM"
    public string HeaderWhere { get; set; } = "";  // "Venue • City, Province"
    public string ImageUrl { get; set; } = "";

    public List<ReviewResponse> Reviews { get; set; } = new();

    // For now: show Add Review only if logged in as CUSTOMER.
    // Later: we can upgrade to true eligibility check using a dedicated API endpoint.
    public bool ShowAddReviewButton { get; set; }
}