namespace EventMaster.Api.DTOs.Reviews;
/// <summary>
/// DTO for creating a reply to a review.
/// </summary>
public class ReplyCreateRequest
{
    public string ReplyText { get; set; } = "";
}

