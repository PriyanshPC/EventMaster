namespace EventMaster.Api.DTOs.Events;

public class EventOccurrenceListItem
{
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public string Category { get; set; } = "";

    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }

    public string VenueName { get; set; } = "";
    public string City { get; set; } = "";
    public string Province { get; set; } = "";

    public decimal Price { get; set; }
    public int RemainingCapacity { get; set; }
    public string Status { get; set; } = "";
}
