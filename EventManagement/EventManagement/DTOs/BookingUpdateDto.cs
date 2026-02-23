namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for updating an existing booking. Contains a single property for the quantity of tickets to be updated. This DTO is used to transfer booking update data from the client to the server in a structured format, allowing for validation and mapping to the Booking entity in the database when updating a booking record.
    /// </summary>
    public class BookingUpdateDto
    {
        public int Quantity { get; set; }
    }
}
