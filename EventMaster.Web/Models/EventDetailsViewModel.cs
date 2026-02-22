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

    // Add Review button is shown only when backend eligibility says the current CUSTOMER can review.
    public bool ShowAddReviewButton { get; set; }
}