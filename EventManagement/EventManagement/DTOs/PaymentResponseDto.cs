namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for payment responses. Contains properties that represent the details of a payment, including the payment ID, booking ID, amount, status, details, and creation timestamp. This DTO is used to transfer payment data from the server to the client in a structured format, allowing for easy consumption and display of payment information in the client application.
    /// </summary>
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
