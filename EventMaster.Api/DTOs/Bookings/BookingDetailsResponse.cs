namespace EventMaster.Api.DTOs.Bookings;

public class BookingDetailsResponse
{
    public int BookingId { get; set; }
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }
    public string EventName { get; set; } = "";
    public string Image { get; set; } = "";
    public string Status { get; set; } = "";
    public string BookingStatus { get; set; } = "";
    public bool CanCancel { get; set; }
    public decimal? RefundedAmount { get; set; }
    public bool IsRefundPending { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public string VenueName { get; set; } = "";
    public string VenueAddress { get; set; } = "";
    public string VenueCity { get; set; } = "";
    public string VenueProvince { get; set; } = "";
    public int NumberOfTickets { get; set; }
    public string? SeatsSelected { get; set; }
    public decimal TotalAmount { get; set; }
    public string TicketNumber { get; set; } = "";
    public string? CardSummary { get; set; }
}
