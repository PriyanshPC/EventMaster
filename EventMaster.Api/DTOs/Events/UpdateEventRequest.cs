namespace EventMaster.Api.DTOs.Events;
/// <summary>
/// Request model for updating an existing event. This is used when modifying the details of an event, such as its name, category, or description. It does not include occurrence-specific information, which is handled separately through occurrence-related endpoints.
/// </summary>
public class UpdateEventRequest
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
}
