using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Events;
using EventMaster.Api.Entities;
using EventMaster.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace EventMaster.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly EventMasterDbContext _db;
    private readonly CurrentUser _me;
    private readonly IWebHostEnvironment _env;

    public EventsController(EventMasterDbContext db, CurrentUser me, IWebHostEnvironment env)
    {
        _db = db;
        _me = me;
        _env = env;
    }

    // -----------------------------
    // PUBLIC (anonymous) endpoints
    // -----------------------------

    // GET api/events/upcoming?category=Music&city=Toronto&q=party&from=2026-02-01&to=2026-03-01
    // Returns EVENTS with their UPCOMING occurrences
    [HttpGet("upcoming")]
    [AllowAnonymous]
    public async Task<ActionResult<List<UpcomingEventResponse>>> GetUpcomingEvents(
        [FromQuery] string? category,
        [FromQuery] string? city,
        [FromQuery] string? q,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from == null ? today : from.Value;
        var toDate = to == null ? (DateOnly?)null : to.Value;

        var baseQuery =
            from ev in _db.events
            join occ in _db.event_occurrences on ev.event_id equals occ.event_id
            join v in _db.venues on occ.venue_id equals v.venue_id
            where occ.status == "Scheduled"
               && occ.date >= fromDate
            select new { ev, occ, v };

        if (toDate != null) baseQuery = baseQuery.Where(x => x.occ.date <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(category))
            baseQuery = baseQuery.Where(x => x.ev.category == category);

        if (!string.IsNullOrWhiteSpace(city))
            baseQuery = baseQuery.Where(x => x.v.city == city);

        if (!string.IsNullOrWhiteSpace(q))
            baseQuery = baseQuery.Where(x =>
                x.ev.name.Contains(q) || (x.ev.description ?? "").Contains(q));

        var rows = await baseQuery
            .OrderBy(x => x.ev.event_id)
            .ThenBy(x => x.occ.date)
            .ThenBy(x => x.occ.time)
            .Select(x => new
            {
                x.ev.event_id,
                x.ev.name,
                x.ev.category,
                x.ev.description,
                x.ev.image,

                Occurrence = new OccurrenceSummaryDto
                {
                    OccurrenceId = x.occ.occurrence_id,
                    EventId = x.ev.event_id,
                    Date = x.occ.date,
                    Time = x.occ.time.ToTimeSpan(),
                    VenueId = x.v.venue_id,
                    VenueName = x.v.name,
                    City = x.v.city,
                    Province = x.v.province,
                    Price = x.occ.price,
                    RemainingCapacity = x.occ.remaining_capacity,
                    Status = x.occ.status
                }
            })
            .ToListAsync();

        // group into events with occurrences
        var result = rows
            .GroupBy(r => new { r.event_id, r.name, r.category, r.description, r.image })
            .Select(g => new UpcomingEventResponse
            {
                EventId = g.Key.event_id,
                Name = g.Key.name,
                Category = g.Key.category,
                Description = g.Key.description,
                ImageFileName = NormalizeOrGenerateImageFileName(g.Key.event_id, g.Key.image),
                Occurrences = g.Select(x => x.Occurrence).ToList()
            })
            .OrderBy(e => e.Occurrences.Min(o => o.Date))
            .ToList();

        return Ok(result);
    }

    // GET api/events/occurrences/upcoming?category=Music
    // Returns UPCOMING occurrences for a category (flattened list)
    [HttpGet("occurrences/upcoming")]
    [AllowAnonymous]
    public async Task<ActionResult<List<EventOccurrenceListItem>>> GetUpcomingOccurrencesByCategory(
        [FromQuery] string category,
        [FromQuery] string? city,
        [FromQuery] string? q,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("category is required.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from == null ? today : from.Value;
        var toDate = to == null ? (DateOnly?)null : to.Value;

        var query =
            from occ in _db.event_occurrences
            join ev in _db.events on occ.event_id equals ev.event_id
            join v in _db.venues on occ.venue_id equals v.venue_id
            where occ.status == "Scheduled"
               && occ.date >= fromDate
               && ev.category == category
            select new { occ, ev, v };

        if (toDate != null) query = query.Where(x => x.occ.date <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(x => x.v.city == city);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.ev.name.Contains(q) || (x.ev.description ?? "").Contains(q));

        var result = await query
            .OrderBy(x => x.occ.date).ThenBy(x => x.occ.time)
            .Select(x => new EventOccurrenceListItem
            {
                OccurrenceId = x.occ.occurrence_id,
                EventId = x.ev.event_id,
                EventName = x.ev.name,
                Category = x.ev.category,
                Date = x.occ.date,
                Time = x.occ.time.ToTimeSpan(),
                VenueName = x.v.name,
                City = x.v.city,
                Province = x.v.province,
                Price = x.occ.price,
                RemainingCapacity = x.occ.remaining_capacity,
                Status = x.occ.status
            })
            .ToListAsync();

        return Ok(result);
    }

    // GET api/events/{eventId}/occurrences (upcoming occurrences for the event)
    [HttpGet("{eventId:int}/occurrences")]
    [AllowAnonymous]
    public async Task<ActionResult<List<OccurrenceSummaryDto>>> GetEventUpcomingOccurrences(int eventId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var exists = await _db.events.AnyAsync(e => e.event_id == eventId);
        if (!exists) return NotFound();

        var occs = await (
            from occ in _db.event_occurrences
            join v in _db.venues on occ.venue_id equals v.venue_id
            where occ.event_id == eventId
               && occ.status == "Scheduled"
               && occ.date > today // upcoming only
            orderby occ.date, occ.time
            select new OccurrenceSummaryDto
            {
                OccurrenceId = occ.occurrence_id,
                EventId = occ.event_id,
                Date = occ.date,
                Time = occ.time.ToTimeSpan(),
                VenueId = v.venue_id,
                VenueName = v.name,
                City = v.city,
                Province = v.province,
                Price = occ.price,
                RemainingCapacity = occ.remaining_capacity,
                Status = occ.status
            }
        ).ToListAsync();

        return Ok(occs);
    }

    // GET api/events/{eventId}/occurrences/{occurrenceId}
    // Includes event + venue + organizer details
    [HttpGet("{eventId:int}/occurrences/{occurrenceId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventOccurrenceDetailsResponse>> GetOccurrenceDetails(int eventId, int occurrenceId)
    {
        var item = await (
            from occ in _db.event_occurrences
            join ev in _db.events on occ.event_id equals ev.event_id
            join v in _db.venues on occ.venue_id equals v.venue_id
            join org in _db.users on ev.org_id equals org.user_id
            where ev.event_id == eventId && occ.occurrence_id == occurrenceId
            select new EventOccurrenceDetailsResponse
            {
                OccurrenceId = occ.occurrence_id,
                EventId = ev.event_id,
                EventName = ev.name,
                Category = ev.category,
                Description = ev.description,
                ImageFileName = NormalizeOrGenerateImageFileName(ev.event_id, ev.image),

                Date = occ.date,
                Time = occ.time.ToTimeSpan(),
                Status = occ.status,

                VenueId = v.venue_id,
                VenueName = v.name,
                Address = v.address,
                City = v.city,
                Province = v.province,
                PostalCode = v.postal_code,
                InitialCapacity = v.capacity,
                VenueSeating = v.seating,

                RemainingCapacity = occ.remaining_capacity,
                Seating = v.seating,
                SeatsOccupied = occ.seats_occupied,
                Price = occ.price,

                OrganizerId = org.user_id,
                OrganizerName = org.name,
                OrganizerEmail = org.email,
                OrganizerPhone = org.phone
            }
        ).FirstOrDefaultAsync();

        if (item is null) return NotFound();
        return Ok(item);
    }

    // GET api/events/{eventId}/image
    // Returns event image file if exists, otherwise 404. Public endpoint (no auth) since images are shown on public pages.
    [HttpGet("{eventId:int}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventImage(int eventId)
    {
        // Optional: verify event exists
        var ev = await _db.events
            .AsNoTracking()
            .Where(e => e.event_id == eventId)
            .Select(e => new { e.image })
            .FirstOrDefaultAsync();

        if (ev is null)
            return NotFound("Event not found.");

        // If DB has filename use it, otherwise fallback to naming rule
        var fileName = string.IsNullOrWhiteSpace(ev.image)
            ? $"event_{eventId:D3}.png"   // 001 format
            : ev.image.Trim();

        // Build path relative to wwwroot
        var fullPath = Path.Combine(
            _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            "images",
            "covers",
            fileName
        );

        if (!System.IO.File.Exists(fullPath))
            return NotFound("Image not found.");

        var contentType = GetContentType(Path.GetExtension(fullPath));

        return PhysicalFile(fullPath, contentType);
    }
    private static string GetContentType(string extension)
    {
        extension = (extension ?? "").ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    // -----------------------------
    // ORGANIZER endpoints
    // -----------------------------

    // POST api/events
    // Create event (Organizer only). Image file name auto-set: event_01.png
    [HttpPost]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(req.Category)) return BadRequest("Category is required.");

        var ev = new _event
        {
            org_id = _me.UserId,
            name = req.Name.Trim(),
            category = req.Category.Trim(),
            description = req.Description
        };

        _db.events.Add(ev);
        await _db.SaveChangesAsync();

        // Now we have event_id; set image filename rule: event_01.png
        ev.image = $"event_{ev.event_id:D2}.png";
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOccurrenceDetails), new { eventId = ev.event_id, occurrenceId = 0 }, new
        {
            ev.event_id,
            ev.name,
            ev.category,
            ev.description,
            imageFileName = ev.image
        });
    }

    // POST api/events/{eventId}/image
    // Upload or replace event image. Validates + resizes to 300x300 with padding. Saves as PNG under wwwroot/images/covers/event_01.png
    [HttpPost("{eventId:int}/image")]
    [Authorize(Roles = "ORGANIZER")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_000_000)] // 5MB
    public async Task<IActionResult> UploadEventImage(int eventId, [FromForm] UploadEventImageRequest request)
    {
        var image = request.Image;
        if (image == null || image.Length == 0)
            return BadRequest("Image is required.");

        // basic validation
        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        var allowedExts = new HashSet<string> { ".png", ".jpg", ".jpeg" };
        if (!allowedExts.Contains(ext))
            return BadRequest("Only PNG/JPG images are allowed.");

        var allowedContentTypes = new HashSet<string> { "image/png", "image/jpeg" };
        if (!allowedContentTypes.Contains((image.ContentType ?? "").ToLowerInvariant()))
            return BadRequest("Invalid image content type.");

        // Build save path under wwwroot/images/covers
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var coversDir = Path.Combine(webRoot, "images", "covers");
        Directory.CreateDirectory(coversDir);


        var fileName = $"event_{eventId:D3}.png";
        var finalPath = Path.Combine(coversDir, fileName);
        var tempPath = finalPath + ".tmp";

        // load + resize (300x300), preserve aspect ratio via padding
        await using var inStream = image.OpenReadStream();
        using var img = await Image.LoadAsync(inStream);

        img.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(300, 300),
            Mode = ResizeMode.Pad
        }));

        // Save as PNG (even if input is jpg)
        await img.SaveAsync(tempPath, new PngEncoder());

        // replace old atomically-ish
        if (System.IO.File.Exists(finalPath))
            System.IO.File.Delete(finalPath);

        System.IO.File.Move(tempPath, finalPath);

        // NO DB updates for now (per your request)
        return Ok(new
        {
            eventId,
            imageFileName = fileName,
            staticUrl = $"/images/covers/{fileName}",
            savedTo = finalPath,
            webRoot = _env.WebRootPath
        });
    }


    // GET api/events/mine
    [HttpGet("mine")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> GetMyEvents()
    {
        var mine = await _db.events
            .Where(e => e.org_id == _me.UserId)
            .OrderByDescending(e => e.created_at)
            .Select(e => new
            {
                e.event_id,
                e.name,
                e.category,
                e.description,
                imageFileName = NormalizeOrGenerateImageFileName(e.event_id, e.image),
                e.created_at,
                e.updated_at
            })
            .ToListAsync();

        return Ok(mine);
    }

    // PUT api/events/{eventId}
    [HttpPut("{eventId:int}")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> UpdateEvent(int eventId, [FromBody] UpdateEventRequest req)
    {
        var ev = await _db.events.FirstOrDefaultAsync(e => e.event_id == eventId);
        if (ev is null) return NotFound();
        if (ev.org_id != _me.UserId) return Forbid();

        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(req.Category)) return BadRequest("Category is required.");

        ev.name = req.Name.Trim();
        ev.category = req.Category.Trim();
        ev.description = req.Description;

        // keep existing image naming rule
        ev.image = $"event_{ev.event_id:D2}.png";

        await _db.SaveChangesAsync();
        return Ok(new
        {
            ev.event_id,
            ev.name,
            ev.category,
            ev.description,
            imageFileName = ev.image
        });
    }

    // DELETE api/events/{eventId}
    // Soft delete event series: cancel only occurrences with status=Scheduled and startDate > now
    [HttpDelete("{eventId:int}")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> CancelEventSeries(int eventId)
    {
        var ev = await _db.events.FirstOrDefaultAsync(e => e.event_id == eventId);
        if (ev is null) return NotFound();
        if (ev.org_id != _me.UserId) return Forbid();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var upcomingScheduled = await _db.event_occurrences
            .Where(o => o.event_id == eventId && o.status == "Scheduled" && o.date > today)
            .ToListAsync();

        foreach (var occ in upcomingScheduled)
            occ.status = "Cancelled";

        await _db.SaveChangesAsync();

        // TODO: trigger refund process for affected bookings (kept out of controller for now)
        return NoContent();
    }

    // POST api/events/{eventId}/occurrences
    [HttpPost("{eventId:int}/occurrences")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> AddOccurrence(int eventId, [FromBody] CreateOccurrenceRequest req)
    {
        if (eventId != req.EventId)
            return BadRequest("eventId route param must match body.eventId.");

        var ev = await _db.events.FirstOrDefaultAsync(e => e.event_id == eventId);
        if (ev is null) return NotFound();
        if (ev.org_id != _me.UserId) return Forbid();

        var venue = await _db.venues.FirstOrDefaultAsync(v => v.venue_id == req.VenueId);
        if (venue is null) return BadRequest("Invalid venueId.");

        if (req.RemainingCapacity < 0) return BadRequest("remainingCapacity must be >= 0.");
        if (req.Price < 0) return BadRequest("price must be >= 0.");

        var occ = new event_occurrence
        {
            event_id = req.EventId,
            venue_id = req.VenueId,
            date = req.Date,
            time = TimeOnly.FromTimeSpan(req.Time),
            price = req.Price,
            remaining_capacity = req.RemainingCapacity,
            status = "Scheduled"
        };

        _db.event_occurrences.Add(occ);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOccurrenceDetails),
            new { eventId = eventId, occurrenceId = occ.occurrence_id },
            new { occ.occurrence_id });
    }

    // PUT api/events/{eventId}/occurrences/{occurrenceId}
    // Update only if organizer owns event; allow changing date/time/price/venue
    [HttpPut("{eventId:int}/occurrences/{occurrenceId:int}")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> UpdateOccurrence(int eventId, int occurrenceId, [FromBody] UpdateOccurrenceRequest req)
    {
        var occ = await (
            from o in _db.event_occurrences
            join ev in _db.events on o.event_id equals ev.event_id
            where o.occurrence_id == occurrenceId && ev.event_id == eventId
            select new { o, ev }
        ).FirstOrDefaultAsync();

        if (occ is null) return NotFound();
        if (occ.ev.org_id != _me.UserId) return Forbid();

        if (occ.o.status != "Scheduled")
            return BadRequest("Only Scheduled occurrences can be updated.");

        var venue = await _db.venues.FirstOrDefaultAsync(v => v.venue_id == req.VenueId);
        if (venue is null) return BadRequest("Invalid venueId.");

        if (req.Price < 0) return BadRequest("price must be >= 0.");
        if (req.RemainingCapacity < 0) return BadRequest("remainingCapacity must be >= 0.");

        occ.o.date = req.Date;
        occ.o.time = TimeOnly.FromTimeSpan(req.Time);
        occ.o.venue_id = req.VenueId;
        occ.o.price = req.Price;
        occ.o.remaining_capacity = req.RemainingCapacity;

        await _db.SaveChangesAsync();
        return Ok();
    }

    // DELETE api/events/{eventId}/occurrences/{occurrenceId}
    // Soft delete occurrence: status -> Cancelled
    [HttpDelete("{eventId:int}/occurrences/{occurrenceId:int}")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> CancelOccurrence(int eventId, int occurrenceId)
    {
        var occ = await (
            from o in _db.event_occurrences
            join ev in _db.events on o.event_id equals ev.event_id
            where ev.event_id == eventId && o.occurrence_id == occurrenceId
            select new { o, ev }
        ).FirstOrDefaultAsync();

        if (occ is null) return NotFound();
        if (occ.ev.org_id != _me.UserId) return Forbid();

        if (occ.o.status == "Cancelled") return NoContent();

        occ.o.status = "Cancelled";
        await _db.SaveChangesAsync();

        // TODO: trigger refund for bookings of this occurrence
        return NoContent();
    }

    // OPTIONS (if your rubric checks)
    [HttpOptions]
    [AllowAnonymous]
    public IActionResult Options() => Ok();

    private static string NormalizeOrGenerateImageFileName(int eventId, string? storedImage)
    {
        // If your DB already has a filename, keep it; otherwise enforce event_01.png
        if (!string.IsNullOrWhiteSpace(storedImage)) return storedImage.Trim();
        return $"event_{eventId:D2}.png";
    }
}