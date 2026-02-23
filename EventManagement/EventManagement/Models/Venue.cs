namespace EventTicketManagement.Api.Models
{
    public class Venue
{
    public int VenueId { get; set; }

    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public int Capacity { get; set; }
    public bool Seating { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<EventOccurrence>? Occurrences { get; set; }
}
}