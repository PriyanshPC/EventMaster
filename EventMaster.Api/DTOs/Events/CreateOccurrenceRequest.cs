namespace EventMaster.Api.DTOs.Events;
/// <summary>
/// Request model for creating a new occurrence of an existing event. This is used when adding a new date/time/venue for an event that already exists.
/// </summary>
public class CreateOccurrenceRequest
{
    public int EventId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
    public int VenueId { get; set; }
    public decimal Price { get; set; }
    public int RemainingCapacity { get; set; }
}
