namespace EventMaster.Api.DTOs.Bookings;

public class BookingDetailsResponse
{
    public int BookingId { get; set; }
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }

    public string EventName { get; set; } = "";
    public string Status { get; set; } = "";

    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public string VenueName { get; set; } = "";
    public string VenueAddress { get; set; } = "";
    public string VenueCity { get; set; } = "";

    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }
}
