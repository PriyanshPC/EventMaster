namespace EventMaster.Api.DTOs.Payments;

public class PaymentCreateRequest
{
    public int BookingId { get; set; }

    // coupon is optional
    public string? CouponCode { get; set; }

    public string NameOnCard { get; set; } = "";
    public string CardNumber { get; set; } = ""; // 14 digits
    public string Exp { get; set; } = "";        // MM/YY
    public string Cvv { get; set; } = "";        // 3 digits
    public string PostalCode { get; set; } = "";
}