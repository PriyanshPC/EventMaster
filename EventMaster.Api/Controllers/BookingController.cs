using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Bookings;
using EventMaster.Api.Entities;
using EventMaster.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMaster.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Roles = "CUSTOMER")]
public class BookingsController : ControllerBase
{
    private readonly EventMasterDbContext _db;
    private readonly CurrentUser _me;

    public BookingsController(EventMasterDbContext db, CurrentUser me)
    {
        _db = db;
        _me = me;
    }

    // =========================
    // 1) GET /api/bookings
    // Get all bookings of the logged-in customer
    // =========================
    [HttpGet]
    public async Task<ActionResult<List<BookingResponse>>> GetMyBookings()
    {
        var myUserId = _me.UserId;

        var items = await _db.bookings
            .AsNoTracking()
            .Where(b => b.customer_id == myUserId)
            .OrderByDescending(b => b.created_at)
            .Select(b => new BookingResponse
            {
                BookingId = b.booking_id,
                OccurrenceId = b.occurrence_id,
                CustomerId = b.customer_id,
                Quantity = b.quantity,
                SeatsOccupied = b.seats_occupied,
                Status = b.status,
                TotalAmount = b.total_amount,
                TicketNumber = b.ticket_number,
                CreatedAt = b.created_at,
                UpdatedAt = b.updated_at
            })
            .ToListAsync();

        return Ok(items);
    }

    // =========================
    // 2) GET /api/bookings/{id}
    // Get a specific booking of the logged-in customer
    // =========================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDetailsResponse>> GetMyBookingById(int id)
    {
        var myUserId = _me.UserId;

        var booking = await (
            from b in _db.bookings.AsNoTracking()
            join occ in _db.event_occurrences.AsNoTracking() on b.occurrence_id equals occ.occurrence_id
            join ev in _db.events.AsNoTracking() on occ.event_id equals ev.event_id
            join v in _db.venues.AsNoTracking() on occ.venue_id equals v.venue_id
            where b.booking_id == id && b.customer_id == myUserId
            select new BookingDetailsResponse
            {
                BookingId = b.booking_id,
                OccurrenceId = occ.occurrence_id,
                EventId = ev.event_id,
                EventName = ev.name,
                Status = occ.status,
                Date = occ.date,
                Time = occ.time,
                VenueName = v.name,
                VenueAddress = v.address,
                VenueCity = v.city,
                Quantity = b.quantity,
                SeatsOccupied = b.seats_occupied
            }
        ).FirstOrDefaultAsync();

        if (booking == null)
            return NotFound(new { message = "Booking not found." });

        return Ok(booking);
    }

    // =========================
    // 3) POST /api/bookings
    // Create booking by the logged-in customer
    // - Updates event_occurrence.remaining_capacity
    // - Merges event_occurrence.seats_occupied string
    // - Creates booking ticket number: EM-{year}-{bookingId:D6}
    // =========================
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] BookingCreateRequest req)
    {
        var myUserId = _me.UserId;

        if (req == null)
            return BadRequest(new { message = "Request body is required." });

        if (req.OccurrenceId <= 0)
            return BadRequest(new { message = "OccurrenceId is required." });

        if (req.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than 0." });

        // Transaction to keep booking + occurrence updates consistent
        await using var tx = await _db.Database.BeginTransactionAsync();

        // Load occurrence
        var occ = await _db.event_occurrences
            .FirstOrDefaultAsync(o => o.occurrence_id == req.OccurrenceId);

        if (occ == null)
            return NotFound(new { message = "Event occurrence not found." });

        if (occ.status != "Scheduled")
            return BadRequest(new { message = $"Cannot book an occurrence with status '{occ.status}'." });

        if (occ.remaining_capacity < req.Quantity)
            return BadRequest(new { message = "Not enough remaining capacity." });

        // Seats logic (optional)
        // Booking DTO might have Seats list OR a comma string depending on your DTO.
        // This controller supports both patterns:
        // - If req.Seats is present, it uses that.
        // - If your BookingCreateRequest instead has SeatsOccupied string, adjust accordingly.
        var requestedSeats = NormalizeSeats(req.Seats);

        if (requestedSeats.Count > 0)
        {
            if (requestedSeats.Count != req.Quantity)
                return BadRequest(new { message = "Seats count must match quantity." });

            // current seats on occurrence
            var occSeats = ParseSeats(occ.seats_occupied);

            // check collisions
            if (requestedSeats.Any(s => occSeats.Contains(s)))
                return Conflict(new { message = "One or more selected seats are already occupied." });

            // merge seats: A1,A2 + C1,C2 => A1,A2,C1,C2
            foreach (var s in requestedSeats)
                occSeats.Add(s);

            occ.seats_occupied = JoinSeats(occSeats);
        }

        // Update remaining capacity
        occ.remaining_capacity -= req.Quantity;

        // Total amount
        var totalAmount = occ.price * req.Quantity;

        // Booking seats string saved in booking row should be seats for this booking only
        var bookingSeatsString = requestedSeats.Count > 0 ? string.Join(",", requestedSeats) : null;

        var b = new booking
        {
            occurrence_id = req.OccurrenceId,
            customer_id = myUserId,
            quantity = req.Quantity,
            seats_occupied = bookingSeatsString,
            status = "Confirmed",
            total_amount = totalAmount,
            ticket_number = "PENDING"
        };

        _db.bookings.Add(b);
        await _db.SaveChangesAsync(); // booking_id generated

        // ticket number: EM-{year}-{bookingId:D6}
        var year = DateTime.UtcNow.Year;
        b.ticket_number = $"EM-{year}-{b.booking_id:D6}";
        await _db.SaveChangesAsync();

        await tx.CommitAsync();

        var response = new BookingResponse
        {
            BookingId = b.booking_id,
            OccurrenceId = b.occurrence_id,
            CustomerId = b.customer_id,
            Quantity = b.quantity,
            SeatsOccupied = b.seats_occupied,
            Status = b.status,
            TotalAmount = b.total_amount,
            TicketNumber = b.ticket_number,
            CreatedAt = b.created_at,
            UpdatedAt = b.updated_at
        };

        return CreatedAtAction(nameof(GetMyBookingById), new { id = b.booking_id }, response);
    }

    // =========================
    // 4) PUT /api/bookings/{id}/cancel
    // Cancel booking (soft delete):
    // - Sets booking.status = Cancelled
    // - Frees capacity (+quantity)
    // - Removes booking seats from occurrence.seats_occupied
    // =========================
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var myUserId = _me.UserId;

        await using var tx = await _db.Database.BeginTransactionAsync();

        var b = await _db.bookings
            .FirstOrDefaultAsync(x => x.booking_id == id && x.customer_id == myUserId);

        if (b == null)
            return NotFound(new { message = "Booking not found." });

        if (b.status == "Cancelled")
            return Ok(new { message = "Booking already cancelled." }); // idempotent

        var occ = await _db.event_occurrences
            .FirstOrDefaultAsync(o => o.occurrence_id == b.occurrence_id);

        if (occ == null)
            return NotFound(new { message = "Event occurrence not found for this booking." });

        // Free capacity
        occ.remaining_capacity += b.quantity;

        // Remove seats from occurrence.seats_occupied (only those in this booking)
        var bookingSeats = ParseSeats(b.seats_occupied);
        if (bookingSeats.Count > 0)
        {
            var occSeats = ParseSeats(occ.seats_occupied);

            foreach (var s in bookingSeats)
                occSeats.Remove(s);

            occ.seats_occupied = JoinSeats(occSeats);
        }

        b.status = "Cancelled";

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "Booking cancelled." });
    }

    // =========================
    // 5) POST /api/bookings/{id}/cancel-refund
    // Customer cancels booking + refund is processed atomically
    // Rules:
    // - Only Scheduled occurrence
    // - Must be >= 24 hours before start
    // Refund:
    // - Refund = last SUCCESS payment amount * 0.85
    // - Insert payments row: status=Refunded, amount = -refund
    // =========================
    [HttpPost("{id:int}/cancel-refund")]
    public async Task<IActionResult> CancelAndRefund(int id)
    {
        var myUserId = _me.UserId;

        await using var tx = await _db.Database.BeginTransactionAsync();

        var b = await _db.bookings
            .FirstOrDefaultAsync(x => x.booking_id == id && x.customer_id == myUserId);

        if (b == null)
            return NotFound(new { message = "Booking not found." });

        if (b.status == "Cancelled")
            return Ok(new { message = "Booking already cancelled." }); // idempotent

        var occ = await _db.event_occurrences
            .FirstOrDefaultAsync(o => o.occurrence_id == b.occurrence_id);

        if (occ == null)
            return NotFound(new { message = "Event occurrence not found for this booking." });

        // Customer can cancel only if occurrence is Scheduled
        if (occ.status != "Scheduled")
            return BadRequest(new { message = $"Cannot cancel a booking for an occurrence with status '{occ.status}'." });

        // 24-hour rule
        var occStart = occ.date.ToDateTime(occ.time); // DateOnly + TimeOnly
        var now = DateTime.UtcNow;

        // NOTE: This assumes occurrence date/time is treated as UTC in your backend.
        // If your app treats occurrence time as local time, swap UtcNow -> Now consistently across the app.
        var hoursUntil = (occStart - now).TotalHours;
        if (hoursUntil < 24)
            return BadRequest(new { message = "Bookings can only be cancelled at least 24 hours before the event starts." });

        // Find latest SUCCESS payment for this booking
        var paid = await _db.payments
            .Where(p => p.booking_id == b.booking_id && p.status == "Success")
            .OrderByDescending(p => p.created_at)
            .FirstOrDefaultAsync();

        if (paid == null)
            return BadRequest(new { message = "No successful payment found for this booking. Refund cannot be processed." });

        // Cancel booking: free capacity + remove seats
        occ.remaining_capacity += b.quantity;

        var bookingSeats = ParseSeats(b.seats_occupied);
        if (bookingSeats.Count > 0)
        {
            var occSeats = ParseSeats(occ.seats_occupied);
            foreach (var s in bookingSeats)
                occSeats.Remove(s);

            occ.seats_occupied = JoinSeats(occSeats);
        }

        b.status = "Cancelled";

        // Refund amount: 15% cancellation fee (includes tax), refund stored as negative amount row
        var refund = Math.Round(paid.amount * 0.85m, 2);

        var refundRow = new payment
        {
            booking_id = b.booking_id,
            amount = -refund,
            card = paid.card, // same card used for booking
            status = "Refunded",
            details = "Customer Cancelled (15% fee)",
            created_at = DateTime.UtcNow
        };

        _db.payments.Add(refundRow);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();

        return Ok(new
        {
            message = "Booking cancelled and refund processed.",
            refundedAmount = refund,
            refundPaymentId = refundRow.payment_id
        });
    }


    // =========================
    // 6) GET /api/bookings/dashboard
    // Returns booking cards data without N+1 calls
    // =========================
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardBookings()
    {
        var myUserId = _me.UserId;

        var rows = await (
            from b in _db.bookings.AsNoTracking()
            join occ in _db.event_occurrences.AsNoTracking() on b.occurrence_id equals occ.occurrence_id
            join ev in _db.events.AsNoTracking() on occ.event_id equals ev.event_id
            join v in _db.venues.AsNoTracking() on occ.venue_id equals v.venue_id
            where b.customer_id == myUserId
            orderby occ.date descending, occ.time descending
            select new
            {
                bookingId = b.booking_id,
                occurrenceId = occ.occurrence_id,
                eventId = ev.event_id,

                // Card fields (match Events view style)
                name = ev.name,
                category = ev.category,
                description = ev.description,
                image = ev.image,

                // Instead of price, frontend shows this status
                status = occ.status, // Scheduled / Cancelled / Completed

                // Occurrence details
                date = occ.date,
                time = occ.time,
                venueName = v.name,
                venueCity = v.city,

                // Booking details
                quantity = b.quantity,
                seatsOccupied = b.seats_occupied,
                bookingStatus = b.status,
                ticketNumber = b.ticket_number,
                createdAt = b.created_at
            }
        ).ToListAsync();

        // Upcoming vs Past split is easier in frontend (needs "now"), but we can return startDateTime too:
        var withStart = rows.Select(r => new
        {
            r.bookingId,
            r.occurrenceId,
            r.eventId,
            r.name,
            r.category,
            r.description,
            r.image,
            r.status,
            r.date,
            r.time,
            startDateTimeUtc = r.date.ToDateTime(r.time), // same note about timezone
            r.venueName,
            r.venueCity,
            r.quantity,
            r.seatsOccupied,
            r.bookingStatus,
            r.ticketNumber,
            r.createdAt
        });

        return Ok(withStart);
    }

    // =========================
    // Seat helpers
    // =========================
    private static List<string> NormalizeSeats(IEnumerable<string>? seats)
    {
        if (seats == null) return new List<string>();

        return seats
            .Select(s => (s ?? "").Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static HashSet<string> ParseSeats(string? seats)
    {
        if (string.IsNullOrWhiteSpace(seats))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return seats
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string? JoinSeats(HashSet<string> seats)
    {
        if (seats.Count == 0) return null;

        // stable ordering helps debugging; storage order isn't critical.
        var ordered = seats.OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
        return string.Join(",", ordered);
    }
}