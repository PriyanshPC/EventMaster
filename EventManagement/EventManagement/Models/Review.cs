namespace EventTicketManagement.Api.Models
{
    public class Review
{
    public int ReviewId { get; set; }
    public int OccurrenceId { get; set; }
    public int CustomerId { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public EventOccurrence? Occurrence { get; set; }
    public User? Customer { get; set; }

    public ICollection<Reply>? Replies { get; set; }
}

}
