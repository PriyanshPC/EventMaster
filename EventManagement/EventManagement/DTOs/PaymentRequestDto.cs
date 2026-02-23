namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for payment requests. Contains properties for the booking ID and the card information used for payment. This DTO is used to transfer payment request data from the client to the server in a structured format, allowing for validation and processing of payment information when a customer attempts to make a payment for a booking.
    /// </summary>
    public class PaymentRequestDto
    {
        public int BookingId { get; set; }
        public string Card { get; set; } = null!;
    }
}
