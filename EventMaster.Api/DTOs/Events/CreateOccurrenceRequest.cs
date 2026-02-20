namespace EventMaster.Api.DTOs.Events;

public class CreateOccurrenceRequest
{
    public int EventId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
    public int VenueId { get; set; }
    public decimal Price { get; set; }
    public int RemainingCapacity { get; set; }
}
