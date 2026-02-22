namespace EventMaster.Web.Services.ApiDtos;

public class ReviewCreateRequestDto
{
    public int EventId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
