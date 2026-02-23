/// <summary>
/// Data Transfer Object (DTO) for creating a new booking. Contains properties for the occurrence ID and the quantity of tickets to be booked. This DTO is used to transfer booking creation data from the client to the server in a structured format, allowing for validation and mapping to the Booking entity in the database.
/// </summary>
public class BookingCreateDto
{
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
}
