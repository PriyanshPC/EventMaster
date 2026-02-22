using EventMaster.Web.Services.ApiDtos;
using System.ComponentModel.DataAnnotations;

namespace EventMaster.Web.Models;

public class BookingIntentModel
{
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
    public List<string> Seats { get; set; } = new();
}

public class BookingPageViewModel
{
    public int EventId { get; set; }
    public int OccurrenceId { get; set; }
    public EventOccurrenceDetailsResponse Details { get; set; } = new();
    public string HeaderWhen { get; set; } = "";
    public string HeaderWhere { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public List<string> RequestedSeats { get; set; } = new();
    public HashSet<string> OccupiedSeats { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public BookingFormInput Input { get; set; } = new();
}

public class BookingFormInput
{
    [Range(1, 10)]
    public int Quantity { get; set; } = 1;
    public List<string> Seats { get; set; } = new();
}

public class PaymentPageViewModel
{
    public BookingIntentModel Intent { get; set; } = new();
    public EventOccurrenceDetailsResponse Details { get; set; } = new();
    public string HeaderWhen { get; set; } = "";
    public string HeaderWhere { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public PaymentFormInput Input { get; set; } = new();
}

public class PaymentFormInput
{
    [Required] public string NameOnCard { get; set; } = "";
    [Required] public string CardNumber { get; set; } = "";
    [Required] public string Exp { get; set; } = "";
    [Required] public string Cvv { get; set; } = "";
    [Required] public string PostalCode { get; set; } = "";
    public string? DiscountCoupon { get; set; }
}

public class ReviewCreatePageViewModel
{
    public int EventId { get; set; }
    public int? OccurrenceId { get; set; }
    public EventOccurrenceDetailsResponse Details { get; set; } = new();
    public string HeaderWhen { get; set; } = "";
    public string HeaderWhere { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public List<ReviewResponse> Reviews { get; set; } = new();
    public ReviewFormInput Input { get; set; } = new();
}

public class ReviewFormInput
{
    [Range(1, 5)]
    public int Rating { get; set; } = 5;
    [StringLength(1000)]
    public string? Comment { get; set; }
}
