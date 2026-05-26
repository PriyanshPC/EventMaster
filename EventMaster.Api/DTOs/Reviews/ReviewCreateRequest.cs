namespace EventMaster.Api.DTOs.Reviews;
/// <summary>
/// DTO for creating a review for an event.
/// </summary>
public class ReviewCreateRequest
{
    public int EventId { get; set; }
    public int Rating { get; set; }          // 1..5
    public string? Comment { get; set; }
}