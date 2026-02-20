namespace EventMaster.Api.DTOs.Events;

public class CreateEventRequest
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
}
