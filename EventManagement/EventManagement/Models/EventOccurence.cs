using System.ComponentModel.DataAnnotations;

namespace EventTicketManagement.Api.Models
{
public class EventOccurrence
{
    [Key]
    public int OccurrenceId { get; set; }
    public int EventId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    public int VenueId { get; set; }
    public decimal Price { get; set; }

    public int RemainingCapacity { get; set; }
    public string? SeatsOccupied { get; set; }

    public string Status { get; set; } = null!; // Scheduled / Cancelled / Completed

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Event? Event { get; set; }
    public Venue? Venue { get; set; }

    public ICollection<Booking>? Bookings { get; set; }
}
}