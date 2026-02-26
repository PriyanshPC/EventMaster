namespace EventMaster.Api.DTOs.Events;
/// <summary>
/// Request model for uploading or updating the image associated with an event. This is used when adding a new image to an event or replacing an existing one. The image is expected to be sent as a multipart/form-data request, and the IFormFile property allows for easy handling of the uploaded file in the API controller.
/// </summary>
public class UploadEventImageRequest
{
    public IFormFile Image { get; set; } = default!;
}