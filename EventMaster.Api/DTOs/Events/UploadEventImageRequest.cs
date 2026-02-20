namespace EventMaster.Api.DTOs.Events;

public class UploadEventImageRequest
{
    public IFormFile Image { get; set; } = default!;
}