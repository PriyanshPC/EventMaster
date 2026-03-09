namespace EventMaster.Api.DTOs.Events;
/// <summary>
/// Request model for creating a new event.
/// </summary>
public class CreateEventRequest
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
}
