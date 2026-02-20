namespace EventMaster.Api.DTOs.Events;

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