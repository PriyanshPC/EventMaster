namespace EventMaster.Web.Services.ApiDtos;

public class PaymentCreateRequestDto
{
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
    public List<string>? Seats { get; set; }

    public string? CouponCode { get; set; }
    public string NameOnCard { get; set; } = "";
    public string CardNumber { get; set; } = "";
    public string Exp { get; set; } = "";
    public string Cvv { get; set; } = "";
    public string PostalCode { get; set; } = "";
}

public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string? Card { get; set; }
    public string Status { get; set; } = "";
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookingFinalizeResponseDto
{
    public int BookingId { get; set; }
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
    public string? SeatsOccupied { get; set; }
    public string TicketNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public PaymentResponseDto Payment { get; set; } = new();
}
