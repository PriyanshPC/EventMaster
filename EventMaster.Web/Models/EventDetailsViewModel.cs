using EventMaster.Web.Services.ApiDtos;

namespace EventMaster.Web.Models;
/// <summary>
///  ViewModel for the event details page, which shows detailed information about a specific event occurrence, including event description, date/time, venue, reviews, and options to book or add a review if eligible. Contains properties for all the relevant information needed to display the event details and control the UI elements on the page (e.g. whether to show the "Add Review" button). This ViewModel is populated by calling the Events API to fetch the event occurrence details and the Reviews API to fetch existing reviews and check review eligibility for the current user.
/// </summary>
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