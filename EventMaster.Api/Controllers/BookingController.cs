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