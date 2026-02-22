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
    public async Task<ActionResult<BookingResponse>> GetMyBookingById(int id)
    {
        var myUserId = _me.UserId;

        var booking = await _db.bookings
            .AsNoTracking()
            .Where(b => b.booking_id == id && b.customer_id == myUserId)
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
            .FirstOrDefaultAsync();

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
    // 2b) GET /api/bookings/{id}/details
    // Booking details for customer dashboard page
    // =========================
    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<BookingDetailsResponse>> GetMyBookingDetailsById(int id)
    {
        var myUserId = _me.UserId;

        var details = await (
            from b in _db.bookings.AsNoTracking()
            join occ in _db.event_occurrences.AsNoTracking() on b.occurrence_id equals occ.occurrence_id
            join ev in _db.events.AsNoTracking() on occ.event_id equals ev.event_id
            join v in _db.venues.AsNoTracking() on occ.venue_id equals v.venue_id
            where b.booking_id == id && b.customer_id == myUserId
            select new
            {
                Booking = b,
                Occurrence = occ,
                Event = ev,
                Venue = v
            }
        ).FirstOrDefaultAsync();

        if (details == null)
            return NotFound(new { message = "Booking not found." });

        var payment = await _db.payments
            .AsNoTracking()
            .Where(p => p.booking_id == id && (p.status == "Success" || p.status == "Refunded"))
            .OrderByDescending(p => p.created_at)
            .FirstOrDefaultAsync();

        return Ok(new BookingDetailsResponse
        {
            BookingId = details.Booking.booking_id,
            EventId = details.Event.event_id,
            OccurrenceId = details.Occurrence.occurrence_id,
            EventName = details.Event.name,
            Image = details.Event.image,
            Status = details.Occurrence.status,
            BookingStatus = details.Booking.status,
            Date = details.Occurrence.date,
            Time = details.Occurrence.time,
            VenueName = details.Venue.name,
            VenueAddress = details.Venue.address,
            VenueCity = details.Venue.city,
            VenueProvince = details.Venue.province,
            NumberOfTickets = details.Booking.quantity,
            SeatsSelected = details.Booking.seats_occupied,
            TotalAmount = details.Booking.total_amount,
            TicketNumber = details.Booking.ticket_number,
            CardSummary = payment?.card,
            CanCancel = details.Booking.status == "Confirmed"
                && details.Occurrence.status == "Scheduled"
                && details.Occurrence.date.ToDateTime(details.Occurrence.time) - DateTime.UtcNow >= TimeSpan.FromHours(24),
            RefundedAmount = payment != null && payment.status == "Refunded" ? Math.Abs(payment.amount) : null,
            IsRefundPending = payment != null && payment.status == "Refunded"
        });
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
    // - Refund = last SUCCESS payment amount * 1.00
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

        // Refund amount: 85% refund (15% cancellation deduction), stored as a negative amount payment row
        var refund = Math.Round(paid.amount * 0.85m, 2);

        var refundRow = new payment
        {
            booking_id = b.booking_id,
            amount = -refund,
            card = paid.card, // same card used for booking
            status = "Refunded",
            details = "Customer Cancelled - 85% Refund (15% deduction)",
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
    public async Task<ActionResult<List<BookingDashboardCardResponse>>> GetDashboardBookings()
    {
        var myUserId = _me.UserId;

        var rows = await (
            from b in _db.bookings.AsNoTracking()
            join occ in _db.event_occurrences.AsNoTracking() on b.occurrence_id equals occ.occurrence_id
            join ev in _db.events.AsNoTracking() on occ.event_id equals ev.event_id
            join v in _db.venues.AsNoTracking() on occ.venue_id equals v.venue_id
            where b.customer_id == myUserId
            orderby occ.date descending, occ.time descending
            select new BookingDashboardCardResponse
            {
                BookingId = b.booking_id,
                OccurrenceId = occ.occurrence_id,
                EventId = ev.event_id,
                Name = ev.name,
                Category = ev.category,
                Description = ev.description,
                Image = ev.image,
                Status = occ.status,
                Date = occ.date,
                Time = occ.time,
                StartDateTimeUtc = occ.date.ToDateTime(occ.time),
                VenueName = v.name,
                VenueCity = v.city,
                Quantity = b.quantity,
                SeatsOccupied = b.seats_occupied,
                BookingStatus = b.status,
                TicketNumber = b.ticket_number,
                CreatedAt = b.created_at
            }
        ).ToListAsync();

        return Ok(rows);
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