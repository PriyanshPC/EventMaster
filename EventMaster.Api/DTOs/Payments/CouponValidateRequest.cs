namespace EventMaster.Api.DTOs.Payments;

public class CouponValidateRequest
{
    public string Code { get; set; } = "";
    public decimal Amount { get; set; }
}