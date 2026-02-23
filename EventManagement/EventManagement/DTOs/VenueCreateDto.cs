namespace EventTicketManagement.Api.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating a new venue. Contains properties for the venue name, address, city, province, postal code, capacity, and seating availability. This DTO is used to transfer venue creation data from the client to the server in a structured format, allowing for validation and mapping to the Venue entity in the database when creating a new venue record.
    /// </summary>
    public class VenueCreateDto
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public int Capacity { get; set; }
        public bool Seating { get; set; }
    }
}
