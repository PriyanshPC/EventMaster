namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating a new event. Contains properties for the event name, category, description, venue ID, organizer ID, and price. This DTO is used to transfer event creation data from the client to the server in a structured format, allowing for validation and mapping to the Event entity in the database when creating a new event record.
    /// </summary>
    public class EventCreateDto
    {
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int VenueId { get; set; }
        public int OrganizerId { get; set; }
        public decimal Price { get; set; }
    }
}
