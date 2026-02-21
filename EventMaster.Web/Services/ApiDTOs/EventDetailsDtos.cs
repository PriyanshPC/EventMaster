namespace EventMaster.Web.Services.ApiDtos;

public class EventOccurrenceDetailsResponse
{
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public string ImageFileName { get; set; } = "";

    public DateOnly Date { get; set; }
    public TimeSpan Time { get; set; }
    public string Status { get; set; } = "";

    public int VenueId { get; set; }
    public string VenueName { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Province { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public int InitialCapacity { get; set; }
    public bool VenueSeating { get; set; }

    public int RemainingCapacity { get; set; }
    public bool Seating { get; set; }
    public string? SeatsOccupied { get; set; }
    public decimal Price { get; set; }

    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = "";
    public string OrganizerEmail { get; set; } = "";
    public string? OrganizerPhone { get; set; }
}

public class ReviewResponse
{
    public int ReviewId { get; set; }
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }

    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";

    public int Rating { get; set; } // 1..5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<ReplyResponse> Replies { get; set; } = new();
}

public class ReplyResponse
{
    public int ReplyId { get; set; }
    public int ReviewId { get; set; }
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = "";
    public string ReplyText { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}