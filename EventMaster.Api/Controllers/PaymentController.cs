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
/// <summary>
/// Controller for handling payments and booking finalization. This includes validating coupons, processing payments through the
/// payment emulator, and creating bookings in a concurrency-safe manner to prevent overselling. All endpoints require authentication since they involve sensitive payment and booking information.
/// </summary>
public class PaymentsController : ControllerBase
{
    private readonly EventMasterDbContext _db;
    private readonly PaymentEmulatorStore _store;

    public PaymentsController(EventMasterDbContext db, PaymentEmulatorStore store)
    {
        _db = db;
        _store = store;
    }
/// <summary>
/// Get payment details by booking ID. Only the customer who owns the booking can access this information. Returns 404 if booking or payment not found, 403 if user tries to access someone else's booking.
/// </summary>
/// <param name="bookingId"></param>
/// <returns></returns>
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
    /// <summary>
    /// Validate a coupon code against the payment emulator's list of coupons. Checks if the coupon exists, is active, not expired, and meets minimum amount requirements. Returns whether the coupon is valid, any applicable discount, and the final amount after discount. This endpoint can be used by the frontend to provide real-time feedback to users when they enter a coupon code during checkout. It does not require authentication since we want to allow users to check coupons before they log in or create an account. However, it could be rate-limited to prevent abuse (e.g. brute-forcing coupon codes).
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
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
/// <summary>
/// Finalize a booking by processing payment and creating a booking record in the database. This endpoint performs several critical functions in a single transaction to ensure data integrity and prevent overselling:
/// </summary>
/// <param name="req"></param>
/// <returns></returns>
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
            if (normalizedSeats.Count == 0)
                return Conflict(new { message = "Seat selection required for the booking." });

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
            if (normalizedSeats.Count > 0)
                return Conflict(new { message = "Seat selection is not available for this venue." });
        }

        occ.remaining_capacity -= req.Quantity;

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

    /// <summary>
    /// Helper method to validate and apply a coupon code. Checks if the coupon exists, is active, not expired, and meets minimum amount requirements. Calculates the discount based on the coupon type (percent or fixed) and returns the final amount after applying the discount. This method is used by both the coupon validation endpoint and the booking finalization endpoint to ensure consistent coupon logic across the application.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Helper method to convert payment entity to response DTO. This abstracts away the internal structure of the payment entity and allows us to control exactly what information is returned to the client. For example, we can choose to mask certain fields or format the data differently if needed. This also keeps our controller actions cleaner and more focused on handling HTTP requests/responses rather than data transformation logic.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
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

/// <summary>
/// Helper method to get the authenticated user's ID from the JWT claims. Returns null if the claim is not present or cannot be parsed as an integer. This is used to identify the user making the request and ensure they can only access their own bookings and payments. We check both "user_id" and ClaimTypes.NameIdentifier to be flexible with different token configurations, but ideally we should standardize on one claim for user ID in our authentication implementation.
/// </summary>
/// <returns></returns>
    private int? GetUserIdOrNull()
    {
        var raw = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }

    private static string NormalizePostal(string v) => Regex.Replace(v ?? "", @"\s+", "").Trim().ToUpperInvariant();

/// <summary>
/// Helper method to validate the input for the booking finalization endpoint. This checks that all required fields are present and in the correct format before we attempt to process the payment or create a booking. By validating early, we can return clear error messages to the client and avoid unnecessary database operations or payment processing attempts with invalid data. This also helps keep our controller action cleaner by abstracting the validation logic into a separate method.
/// </summary>
/// <param name="req"></param>
/// <returns></returns>
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
