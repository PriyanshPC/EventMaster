namespace EventMaster.Api.DTOs.Events;

public class UpdateEventRequest
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
}
