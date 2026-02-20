namespace EventMaster.Api.DTOs.Payments;

public class CouponValidateResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = "";
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}