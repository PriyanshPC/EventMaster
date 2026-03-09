namespace EventMaster.Api.DTOs.Payments;
/// <summary>
/// DTO for the response of a payment operation. It includes details about the payment such as the payment ID, booking ID, amount, masked card information, status, additional details, and the timestamp of when the payment was created. This is used to provide feedback to the client about the outcome of a payment transaction.
/// </summary>
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
/// <summary>
/// DTO for the response of finalizing a booking. It includes details about the booking such as the booking ID, occurrence ID, quantity of tickets, occupied seats, ticket number, total amount, and the associated payment information. This is used to provide comprehensive feedback to the client about the finalized booking and its payment status after a successful transaction.
/// </summary>
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
