namespace EventTicketManagement.Api.Models
{
    public class Booking
{
    public int BookingId { get; set; }
    public int OccurrenceId { get; set; }
    public int CustomerId { get; set; }

    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }

    public string Status { get; set; } = null!; // Confirmed / Cancelled
    public decimal TotalAmount { get; set; }
    public string TicketNumber { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public EventOccurrence? Occurrence { get; set; }
    public User? Customer { get; set; }

    public ICollection<Payment>? Payments { get; set; }
}

}
