namespace EventMaster.Api.DTOs.Bookings;

public class BookingResponse
{
    public int BookingId { get; set; }
    public int OccurrenceId { get; set; }
    public int CustomerId { get; set; }
    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string TicketNumber { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}