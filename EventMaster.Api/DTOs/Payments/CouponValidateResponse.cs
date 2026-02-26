namespace EventMaster.Api.DTOs.Payments;
/// <summary>
/// DTO for the response of validating a coupon code. It indicates whether the coupon is valid, provides a message for the validation result, and includes details about the original amount, discount amount, and final amount after applying the coupon. This is used to inform the client about the outcome of the coupon validation process before proceeding with a payment.
/// </summary>
public class CouponValidateResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = "";
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}