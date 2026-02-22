namespace EventMaster.Web.Models;

public class BookingDetailsViewModel
{
    public int BookingId { get; set; }
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }
    public string EventName { get; set; } = "";
    public string? Image { get; set; }
    public string Status { get; set; } = "";
    public string DateTimeLine { get; set; } = "";
    public string VenueLine { get; set; } = "";
    public int NumberOfTickets { get; set; }
    public string SeatsSelected { get; set; } = "General Admission";
    public string CardSummary { get; set; } = "N/A";
    public decimal TotalAmount { get; set; }
    public string TicketNumber { get; set; } = "";
    public bool CanCancel { get; set; }
    public bool IsPastBooking { get; set; }
    public bool ShowAddReviewButton { get; set; }
    public string? AddReviewUrl { get; set; }
    public decimal? RefundedAmount { get; set; }
    public bool IsRefundPending { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}
