namespace EventMaster.Api.DTOs.Reviews;

public class ReviewResponse
{
    public int ReviewId { get; set; }
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }

    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";

    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<ReplyResponse> Replies { get; set; } = new();
}
