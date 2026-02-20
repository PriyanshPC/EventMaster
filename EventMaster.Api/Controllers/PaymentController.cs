using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Payments;
using EventMaster.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace EventMaster.Api.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly EventMasterDbContext _db;
    private readonly PaymentEmulatorStore _store;

    public PaymentsController(EventMasterDbContext db, PaymentEmulatorStore store)
    {
        _db = db;
        _store = store;
    }

    // GET: api/payments/booking/{bookingId}
    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<PaymentResponse>> GetByBookingId(int bookingId)
    {
        var userId = GetUserIdOrNull();
        if (userId is null) return Unauthorized();

        // Ownership check (booking must belong to logged-in customer)
        var bookingOwnerId = await _db.bookings
            .AsNoTracking()
            .Where(b => b.booking_id == bookingId)
            .Select(b => b.customer_id)
            .FirstOrDefaultAsync();

        if (bookingOwnerId == 0)
            return NotFound(new { message = "Booking not found." });

        if (bookingOwnerId != userId.Value)
            return Forbid();

        // If 1:1, there should be only one row.
        // If you allow multiple attempts (Failed + Success), pick the latest by created_at.
        var payment = await _db.payments
            .AsNoTracking()
            .Where(p => p.booking_id == bookingId)
            .OrderByDescending(p => p.created_at)
            .FirstOrDefaultAsync();

        if (payment is null)
            return NotFound(new { message = "Payment not found for this booking." });

        return Ok(ToResponse(payment));
    }


    // -------------------------
    // POST: api/payments/validate-coupon
    // Used when user "applies" coupon BEFORE paying.
    // If invalid => frontend should block payment attempt.
    // -------------------------
    [HttpPost("validate-coupon")]
    public async Task<ActionResult<CouponValidateResponse>> ValidateCoupon([FromBody] CouponValidateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return BadRequest(new { message = "Coupon code is required." });

        if (req.Amount < 0)
            return BadRequest(new { message = "Amount must be >= 0." });

        var (ok, message, discount, final) = await TryApplyCoupon(req.Code.Trim(), req.Amount);

        return Ok(new CouponValidateResponse
        {
            IsValid = ok,
            Message = message,
            OriginalAmount = req.Amount,
            DiscountAmount = discount,
            FinalAmount = final
        });
    }

    // -------------------------
    // POST: api/payments
    // Attempts payment:
    // 1) Validate coupon (if provided) - if invalid => DO NOT proceed
    // 2) Match card details against payment.json
    // 3) If insufficient => write FAILED payment row
    // 4) If incorrect details => write FAILED payment row
    // 5) If success => deduct balance (write back to payment.json) + write SUCCESS payment row
    // -------------------------
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create([FromBody] PaymentCreateRequest req)
    {
        var userId = GetUserIdOrNull();
        if (userId is null) return Unauthorized();

        var validationError = ValidateCardInput(req);
        if (validationError is not null)
            return BadRequest(new { message = validationError });

        var booking = await _db.bookings
            .FirstOrDefaultAsync(b => b.booking_id == req.BookingId);

        if (booking is null)
            return NotFound(new { message = "Booking not found." });

        // Ownership check: customer can pay only for their booking
        if (booking.customer_id != userId.Value)
            return Forbid();

        var amountToCharge = booking.total_amount;

        // Coupon validation FIRST: if invalid, do not proceed
        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var (ok, message, _, final) = await TryApplyCoupon(req.CouponCode.Trim(), amountToCharge);
            if (!ok)
                return BadRequest(new { message }); // do not proceed with payment

            amountToCharge = final;
        }

        // Read payment.json
        var data = await _store.ReadAsync();

        // Card match must match all fields (per your requirement)
        var matchedCard = data.Card_Details.FirstOrDefault(c =>
            string.Equals(c.Name_On_Card, req.NameOnCard.Trim(), StringComparison.OrdinalIgnoreCase) &&
            c.Card_Number == req.CardNumber.Trim() &&
            c.Exp == req.Exp.Trim() &&
            c.Cvv == req.Cvv.Trim() &&
            string.Equals(c.Postal_Code, NormalizePostal(c.Postal_Code), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(NormalizePostal(req.PostalCode), NormalizePostal(c.Postal_Code), StringComparison.OrdinalIgnoreCase)
        );

        var masked = PaymentEmulatorStore.MaskCard(req.CardNumber.Trim(), req.Exp.Trim());

        // Incorrect card details => store FAILED row
        if (matchedCard is null)
        {
            var failed = await CreatePaymentRowAsync(req.BookingId, amountToCharge, masked, "Failed", "Incorrect Card Details");
            return BadRequest(ToResponse(failed));
        }

        // Insufficient balance => store FAILED row
        if (matchedCard.Amount_Balance < amountToCharge)
        {
            var failed = await CreatePaymentRowAsync(req.BookingId, amountToCharge, masked, "Failed", "Insufficient Balance");
            return BadRequest(ToResponse(failed));
        }

        // Success => deduct balance and write json back
        matchedCard.Amount_Balance -= amountToCharge;
        await _store.WriteAsync(data);

        var success = await CreatePaymentRowAsync(req.BookingId, amountToCharge, masked, "Success", "Approved");
        return Ok(ToResponse(success));
    }

    // -------------------------
    // Helpers
    // -------------------------
    private async Task<(bool ok, string message, decimal discount, decimal final)> TryApplyCoupon(string code, decimal amount)
    {
        var data = await _store.ReadAsync();
        var coupon = data.Coupons.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        if (coupon is null)
            return (false, "Invalid coupon code.", 0, amount);

        if (!coupon.Is_Active)
            return (false, "Coupon is not active.", 0, amount);

        if (coupon.Expires_At < DateOnly.FromDateTime(DateTime.UtcNow))
            return (false, "Coupon is expired.", 0, amount);

        if (amount < coupon.Min_Amount)
            return (false, $"Coupon requires minimum amount {coupon.Min_Amount:0.00}.", 0, amount);

        decimal discount = 0;
        if (string.Equals(coupon.Type, "Percent", StringComparison.OrdinalIgnoreCase))
        {
            discount = Math.Round(amount * (coupon.Value / 100m), 2);
        }
        else if (string.Equals(coupon.Type, "Fixed", StringComparison.OrdinalIgnoreCase))
        {
            discount = Math.Round(coupon.Value, 2);
        }
        else
        {
            return (false, "Coupon type is invalid in payment.json.", 0, amount);
        }

        if (discount < 0) discount = 0;
        if (discount > amount) discount = amount;

        var final = amount - discount;
        return (true, "Coupon applied.", discount, final);
    }

    private async Task<payment> CreatePaymentRowAsync(int bookingId, decimal amount, string maskedCard, string status, string details)
    {
        var row = new payment
        {
            booking_id = bookingId,
            amount = amount,
            card = maskedCard,
            status = status,
            details = details,
            created_at = DateTime.UtcNow
        };

        _db.payments.Add(row);
        await _db.SaveChangesAsync();
        return row;
    }

    private static PaymentResponse ToResponse(payment p) => new()
    {
        PaymentId = p.payment_id,
        BookingId = p.booking_id,
        Amount = p.amount,
        Card = p.card,
        Status = p.status,
        Details = p.details,
        CreatedAt = p.created_at
    };

    private int? GetUserIdOrNull()
    {
        // supports either "user_id" or NameIdentifier
        var raw =
            User.FindFirstValue("user_id") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(raw, out var id) ? id : null;
    }

    private static string NormalizePostal(string v)
        => Regex.Replace(v ?? "", @"\s+", "").Trim().ToUpperInvariant();

    private static string? ValidateCardInput(PaymentCreateRequest req)
    {
        if (req.BookingId <= 0) return "BookingId must be valid.";

        if (string.IsNullOrWhiteSpace(req.NameOnCard)) return "NameOnCard is required.";
        if (req.NameOnCard.Length > 100) return "NameOnCard is too long.";

        if (!Regex.IsMatch(req.CardNumber ?? "", @"^\d{14}$"))
            return "CardNumber must be exactly 14 digits.";

        if (!Regex.IsMatch(req.Exp ?? "", @"^(0[1-9]|1[0-2])\/\d{2}$"))
            return "Exp must be in MM/YY format.";

        if (!Regex.IsMatch(req.Cvv ?? "", @"^\d{3}$"))
            return "CVV must be exactly 3 digits.";

        if (string.IsNullOrWhiteSpace(req.PostalCode))
            return "PostalCode is required.";

        return null;
    }
}