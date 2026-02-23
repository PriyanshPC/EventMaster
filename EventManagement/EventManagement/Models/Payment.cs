namespace EventTicketManagement.Api.Models
{
    public class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }

    public decimal Amount { get; set; }
    public string? Card { get; set; }
    public string Status { get; set; } = null!; // Success / Failed
    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }

    public Booking? Booking { get; set; }
}

}
