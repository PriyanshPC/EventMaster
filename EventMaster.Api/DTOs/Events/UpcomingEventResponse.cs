namespace EventMaster.Api.DTOs.Events;
/// <summary>
/// Response model for retrieving a list of upcoming events, including basic event details and a summary of their occurrences. This model is used in endpoints that return a list of events with their upcoming occurrences, providing enough information for users to see what's coming up without overwhelming them with details. Each event includes a list of its occurrences, which contain summary information about the date, time, venue, price, and availability.
/// </summary>
public class UpcomingEventResponse
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public string ImageFileName { get; set; } = "";
    public List<OccurrenceSummaryDto> Occurrences { get; set; } = new();
}
public class OccurrenceSummaryDto
{
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }

    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }

    public int VenueId { get; set; }
    public string VenueName { get; set; } = "";
    public string City { get; set; } = "";
    public string Province { get; set; } = "";

    public decimal Price { get; set; }
    public int RemainingCapacity { get; set; }
    public string Status { get; set; } = "";
}