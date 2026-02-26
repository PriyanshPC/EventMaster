using EventMaster.Web.Services.ApiDtos;

namespace EventMaster.Web.Models;
/// <summary>
/// ViewModel for the organizer occurrence details page, which shows detailed information about a specific event occurrence from the organizer's perspective, including event description, date/time, venue, and options to cancel the occurrence if eligible. Contains properties for all the relevant information needed to display the occurrence details and control the UI elements on the page (e.g. whether to show the "Cancel Occurrence" button). This ViewModel is populated by calling the Events API to fetch the event occurrence details for a given occurrence ID, and it is used to render the organizer occurrence details page with all the necessary information and controls for managing the occurrence.
/// </summary>
public class OrganizerOccurrenceDetailsViewModel
{
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }
    public EventOccurrenceDetailsResponse Details { get; set; } = new();
    public string HeaderWhen { get; set; } = "";
    public string HeaderWhere { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public bool CanCancel { get; set; }
}
