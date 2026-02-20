namespace EventMaster.Api.DTOs.Bookings;

public class BookingCreateRequest
{
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
    public List<string>? Seats { get; set; }
}