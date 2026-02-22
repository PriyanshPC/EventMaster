using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Payments;
using EventMaster.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
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

    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<PaymentResponse>> GetByBookingId(int bookingId)
    {
        var userId = GetUserIdOrNull();
        if (userId is null) return Unauthorized();

        var bookingOwnerId = await _db.bookings.AsNoTracking().Where(b => b.booking_id == bookingId).Select(b => b.customer_id).FirstOrDefaultAsync();
        if (bookingOwnerId == 0) return NotFound(new { message = "Booking not found." });
        if (bookingOwnerId != userId.Value) return Forbid();

        var payment = await _db.payments.AsNoTracking().Where(p => p.booking_id == bookingId).OrderByDescending(p => p.created_at).FirstOrDefaultAsync();
        if (payment is null) return NotFound(new { message = "Payment not found for this booking." });

        return Ok(ToResponse(payment));
    }

    [HttpPost("validate-coupon")]
    public async Task<ActionResult<CouponValidateResponse>> ValidateCoupon([FromBody] CouponValidateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Code)) return BadRequest(new { message = "Coupon code is required." });
        if (req.Amount < 0) return BadRequest(new { message = "Amount must be >= 0." });

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

    // Finalize booking + payment in one transaction to prevent overselling / seat races
    [HttpPost("finalize-booking")]
    public async Task<ActionResult<BookingFinalizeResponse>> FinalizeBooking([FromBody] PaymentCreateRequest req)
    {
        var userId = GetUserIdOrNull();
        if (userId is null) return Unauthorized();

        var validationError = ValidateFinalizeInput(req);
        if (validationError is not null) return BadRequest(new { message = validationError });

        var normalizedSeats = NormalizeSeats(req.Seats);
        var masked = PaymentEmulatorStore.MaskCard(req.CardNumber.Trim(), req.Exp.Trim());

        // Validate card/coupon against emulator first (source of truth for payment success/failure)
        var occSnapshot = await _db.event_occurrences.AsNoTracking().FirstOrDefaultAsync(o => o.occurrence_id == req.OccurrenceId);
        if (occSnapshot is null) return NotFound(new { message = "Event occurrence not found." });

        var amountToCharge = occSnapshot.price * req.Quantity;
        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var (ok, message, _, final) = await TryApplyCoupon(req.CouponCode.Trim(), amountToCharge);
            if (!ok) return BadRequest(new { message });
            amountToCharge = final;
        }

        var data = await _store.ReadAsync();
        var matchedCard = data.Card_Details.FirstOrDefault(c =>
            string.Equals(c.Name_On_Card, req.NameOnCard.Trim(), StringComparison.OrdinalIgnoreCase) &&
            c.Card_Number == req.CardNumber.Trim() &&
            c.Exp == req.Exp.Trim() &&
            c.Cvv == req.Cvv.Trim() &&
            string.Equals(NormalizePostal(req.PostalCode), NormalizePostal(c.Postal_Code), StringComparison.OrdinalIgnoreCase));

        if (matchedCard is null) return BadRequest(new { message = "Incorrect Card Details" });
        if (matchedCard.Amount_Balance < amountToCharge) return BadRequest(new { message = "Insufficient Balance" });

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        // MySQL row lock for concurrency safety
        var occ = await _db.event_occurrences
            .FromSqlInterpolated($"SELECT * FROM event_occurrences WHERE occurrence_id = {req.OccurrenceId} FOR UPDATE")
            .FirstOrDefaultAsync();

        if (occ is null)
            return NotFound(new { message = "Event occurrence not found." });

        if (occ.status != "Scheduled")
            return Conflict(new { message = $"Cannot book an occurrence with status '{occ.status}'." });

        if (occ.remaining_capacity < req.Quantity)
            return Conflict(new { message = "Not enough remaining capacity." });

        var venueSeating = await _db.venues
            .AsNoTracking()
            .Where(v => v.venue_id == occ.venue_id)
            .Select(v => (bool?)v.seating)
            .FirstOrDefaultAsync();

        if (venueSeating is null)
            return Conflict(new { message = "Venue not found for selected occurrence." });

        if (venueSeating.Value)
        {
            if (normalizedSeats.Count != req.Quantity)
                return Conflict(new { message = "Seats count must match quantity." });

            var occSeats = ParseSeats(occ.seats_occupied);
            if (normalizedSeats.Any(s => occSeats.Contains(s)))
                return Conflict(new { message = "One or more selected seats are already occupied." });

            foreach (var seat in normalizedSeats)
                occSeats.Add(seat);

            occ.seats_occupied = JoinSeats(occSeats);
        }
        else
        {
            normalizedSeats.Clear();
        }

        occ.remaining_capacity -= req.Quantity;

        // Deduct card only once lock+capacity checks pass
        matchedCard.Amount_Balance -= amountToCharge;
        await _store.WriteAsync(data);

        var bookingSeats = normalizedSeats.Count > 0 ? string.Join(",", normalizedSeats) : null;
        var booking = new booking
        {
            occurrence_id = req.OccurrenceId,
            customer_id = userId.Value,
            quantity = req.Quantity,
            seats_occupied = bookingSeats,
            status = "Confirmed",
            total_amount = amountToCharge,
            ticket_number = "PENDING"
        };

        _db.bookings.Add(booking);
        await _db.SaveChangesAsync();

        booking.ticket_number = $"EM-{DateTime.UtcNow.Year}-{booking.booking_id:D6}";

        var payment = new payment
        {
            booking_id = booking.booking_id,
            amount = amountToCharge,
            card = masked,
            status = "Success",
            details = "Approved",
            created_at = DateTime.UtcNow
        };

        _db.payments.Add(payment);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new BookingFinalizeResponse
        {
            BookingId = booking.booking_id,
            OccurrenceId = booking.occurrence_id,
            Quantity = booking.quantity,
            SeatsOccupied = booking.seats_occupied,
            TicketNumber = booking.ticket_number,
            TotalAmount = booking.total_amount,
            Payment = ToResponse(payment)
        });
    }

    private async Task<(bool ok, string message, decimal discount, decimal final)> TryApplyCoupon(string code, decimal amount)
    {
        var data = await _store.ReadAsync();
        var coupon = data.Coupons.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        if (coupon is null) return (false, "Invalid coupon code.", 0, amount);
        if (!coupon.Is_Active) return (false, "Coupon is not active.", 0, amount);
        if (coupon.Expires_At < DateOnly.FromDateTime(DateTime.UtcNow)) return (false, "Coupon is expired.", 0, amount);
        if (amount < coupon.Min_Amount) return (false, $"Coupon requires minimum amount {coupon.Min_Amount:0.00}.", 0, amount);

        decimal discount = string.Equals(coupon.Type, "Percent", StringComparison.OrdinalIgnoreCase)
            ? Math.Round(amount * (coupon.Value / 100m), 2)
            : string.Equals(coupon.Type, "Fixed", StringComparison.OrdinalIgnoreCase)
                ? Math.Round(coupon.Value, 2)
                : -1;

        if (discount < 0) return (false, "Coupon type is invalid in payment.json.", 0, amount);
        if (discount > amount) discount = amount;

        return (true, "Coupon applied.", discount, amount - discount);
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
        var raw = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }

    private static string NormalizePostal(string v) => Regex.Replace(v ?? "", @"\s+", "").Trim().ToUpperInvariant();

    private static string? ValidateFinalizeInput(PaymentCreateRequest req)
    {
        if (req.OccurrenceId <= 0) return "OccurrenceId must be valid.";
        if (req.Quantity <= 0 || req.Quantity > 10) return "Quantity must be between 1 and 10.";
        if (string.IsNullOrWhiteSpace(req.NameOnCard)) return "NameOnCard is required.";
        if (!Regex.IsMatch(req.CardNumber ?? "", @"^\d{14}$")) return "CardNumber must be exactly 14 digits.";
        if (!Regex.IsMatch(req.Exp ?? "", @"^(0[1-9]|1[0-2])\/\d{2}$")) return "Exp must be in MM/YY format.";
        if (!Regex.IsMatch(req.Cvv ?? "", @"^\d{3}$")) return "CVV must be exactly 3 digits.";
        if (string.IsNullOrWhiteSpace(req.PostalCode)) return "PostalCode is required.";
        return null;
    }

    private static List<string> NormalizeSeats(List<string>? seats)
        => seats?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim().ToUpperInvariant()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new();

    private static HashSet<string> ParseSeats(string? seats)
        => seats?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new(StringComparer.OrdinalIgnoreCase);

    private static string JoinSeats(HashSet<string> seats)
        => string.Join(',', seats.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
}
