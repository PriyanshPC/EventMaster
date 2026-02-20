namespace EventMaster.Api.DTOs.Events;

public class UpdateOccurrenceRequest
{
    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
    public int VenueId { get; set; }
    public decimal Price { get; set; }
    public int RemainingCapacity { get; set; }
}
