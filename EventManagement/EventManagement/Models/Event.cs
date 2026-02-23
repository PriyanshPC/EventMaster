namespace EventTicketManagement.Api.Models
{
public class Event
{
    public int EventId { get; set; }
    public int OrgId { get; set; }

    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
    public string? Image { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? Organizer { get; set; }
    public ICollection<EventOccurrence>? Occurrences { get; set; }
}
}