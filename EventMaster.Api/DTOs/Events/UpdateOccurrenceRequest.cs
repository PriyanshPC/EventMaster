namespace EventMaster.Api.DTOs.Events;
/// <summary>
/// Request model for updating an existing event occurrence. This is used when modifying the details of a specific occurrence of an event, such as its date, time, venue, price, or remaining capacity. It does not include event-level information, which is handled separately through event-related endpoints.
/// </summary>
public class UpdateOccurrenceRequest
{
    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
    public int VenueId { get; set; }
    public decimal Price { get; set; }
    public int RemainingCapacity { get; set; }
}
