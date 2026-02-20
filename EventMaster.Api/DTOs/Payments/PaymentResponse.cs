namespace EventMaster.Api.DTOs.Payments;

public class PaymentResponse
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string? Card { get; set; }
    public string Status { get; set; } = "";
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}