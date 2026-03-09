namespace EventMaster.Api.DTOs.Payments;
/// <summary>
/// DTO for creating a payment. It includes details about the occurrence, quantity of tickets, optional seat selections, optional coupon code, and payment information such as card details and postal code. This is used to capture all necessary information for processing a payment for an event occurrence.
/// </summary>
public class PaymentCreateRequest
{
    public int OccurrenceId { get; set; }
    public int Quantity { get; set; }
    public List<string>? Seats { get; set; }

    // coupon is optional
    public string? CouponCode { get; set; }

    public string NameOnCard { get; set; } = "";
    public string CardNumber { get; set; } = ""; // 14 digits
    public string Exp { get; set; } = "";        // MM/YY
    public string Cvv { get; set; } = "";        // 3 digits
    public string PostalCode { get; set; } = "";
}
