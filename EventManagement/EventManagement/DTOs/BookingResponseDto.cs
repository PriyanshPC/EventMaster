namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for booking responses. Contains properties that represent the details of a booking, including the booking ID, occurrence ID, customer ID, quantity of tickets, total amount, status, ticket number, and creation timestamp. This DTO is used to transfer booking data from the server to the client in a structured format, allowing for easy consumption and display of booking information in the client application.
    /// </summary>
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public int OccurrenceId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string TicketNumber { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
