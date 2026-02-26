namespace EventMaster.Api.DTOs.Payments;
/// <summary>
/// DTO for validating a coupon code against a specific amount. This is used to check if the coupon is valid and applicable for the given amount before applying it to a payment.
/// </summary>
public class CouponValidateRequest
{
    public string Code { get; set; } = "";
    public decimal Amount { get; set; }
}