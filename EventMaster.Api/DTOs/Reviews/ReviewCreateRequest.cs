namespace EventMaster.Api.DTOs.Reviews;

public class ReviewCreateRequest
{
    public int EventId { get; set; }
    public int Rating { get; set; }          // 1..5
    public string? Comment { get; set; }
}