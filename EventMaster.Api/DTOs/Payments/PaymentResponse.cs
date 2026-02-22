namespace EventMaster.Api.DTOs.Payments;

public class PaymentResponse
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string? Card { get; set; }
    public string Status { get; set; } = "";
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookingFinalizeResponse
{
    public int BookingId { get; set; }
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }
    public string TicketNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public PaymentResponse Payment { get; set; } = new();
}
