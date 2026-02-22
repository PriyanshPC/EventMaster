namespace EventMaster.Web.Services.ApiDtos;

public class OrganizerEventListItemDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<string> OccurrenceStatuses { get; set; } = new();
}

public class OrganizerCreateEventSeriesRequestDto
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public List<OrganizerCreateOccurrenceDto> Occurrences { get; set; } = new();
}

public class OrganizerCreateOccurrenceDto
{
    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
    public int VenueId { get; set; }
    public decimal Price { get; set; }
}

public class OrganizerCreatedEventSeriesResponseDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public string ImageFileName { get; set; } = "";
    public List<int> OccurrenceIds { get; set; } = new();
}

public class VenueResponseDto
{
    public int VenueId { get; set; }
    public string Name { get; set; } = "";
}

public class OrganizerPendingReviewDto
{
    public int ReviewId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReplyCreateRequestDto
{
    public string ReplyText { get; set; } = "";
}
