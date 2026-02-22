using EventMaster.Web.Services.ApiDtos;

namespace EventMaster.Web.Models;

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
