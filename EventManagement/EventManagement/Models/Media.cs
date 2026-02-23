namespace EventTicketManagement.Api.Models
{
    public class Media
{
    public int MediaId { get; set; }
    public int OccurrenceId { get; set; }
    public int UserId { get; set; }

    public string MediaType { get; set; } = null!; // PHOTO / VIDEO
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public EventOccurrence? Occurrence { get; set; }
    public User? User { get; set; }
}

}
