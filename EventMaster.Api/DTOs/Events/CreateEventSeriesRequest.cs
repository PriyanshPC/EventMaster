namespace EventMaster.Api.DTOs.Events;

public class CreateEventSeriesRequest
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public IFormFile? Image { get; set; }
    public List<CreateEventSeriesOccurrenceRequest> Occurrences { get; set; } = new();
}

public class CreateEventSeriesOccurrenceRequest
{
    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
    public int VenueId { get; set; }
    public decimal Price { get; set; }
}
