namespace EventTicketManagement.Api.Models
{
    public class Reply
{
    public int ReplyId { get; set; }
    public int ReviewId { get; set; }
    public int OrganizerId { get; set; }

    public string ReplyText { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Review? Review { get; set; }
    public User? Organizer { get; set; }
}

}
