namespace EventMaster.Api.DTOs.Bookings;
/// <summary>
/// Request model for creating a booking.
/// </summary>
public class BookingCreateRequest
{
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
    public List<string>? Seats { get; set; }
}