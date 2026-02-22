namespace EventMaster.Api.DTOs.Bookings;

public class BookingDashboardCardResponse
{
    public int BookingId { get; set; }
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string Status { get; set; } = "";
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public DateTime StartDateTimeUtc { get; set; }
    public string VenueName { get; set; } = "";
    public string VenueCity { get; set; } = "";
    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }
    public string BookingStatus { get; set; } = "";
    public string TicketNumber { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
