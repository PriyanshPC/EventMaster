namespace EventMaster.Api.DTOs.Reviews;

public class ReplyResponse
{
    public int ReplyId { get; set; }
    public int ReviewId { get; set; }
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = "";
    public string ReplyText { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}