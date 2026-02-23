namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating a new occurrence. Contains properties for the date, time, price, remaining capacity, venue ID, and status of the occurrence. This DTO is used to transfer occurrence creation data from the client to the server in a structured format, allowing for validation and mapping to the Occurrence entity in the database when creating a new occurrence record.
    /// </summary>
    public class OccurrenceCreateDto
    {
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public decimal Price { get; set; }
        public int RemainingCapacity { get; set; }
        public int VenueId { get; set; }
        public string Status { get; set; } = null; // Scheduled / Completed / Cancelled
    }
}
