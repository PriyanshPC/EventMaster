using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Reviews;
using EventMaster.Api.Entities;
using EventMaster.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMaster.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly EventMasterDbContext _db;
    private readonly CurrentUser _me;

    public ReviewController(EventMasterDbContext db, CurrentUser me)
    {
        _db = db;
        _me = me;
    }

    // =========================
    // 1) POST /api/reviews
    // Customer can review ONLY if:
    // - They attended any completed occurrence of the event
    // - They have not already reviewed that attended occurrence
    // =========================
    [HttpPost]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ReviewResponse>> Create([FromBody] ReviewCreateRequest req)
    {
        if (req == null)
            return BadRequest(new { message = "Request body is required." });

        if (req.EventId <= 0)
            return BadRequest(new { message = "EventId is required." });

        if (req.Rating < 1 || req.Rating > 5)
            return BadRequest(new { message = "Rating must be between 1 and 5." });

        var myUserId = _me.UserId;

        var evtExists = await _db.events.AnyAsync(e => e.event_id == req.EventId);
        if (!evtExists)
            return NotFound(new { message = "Event not found." });

        var eligibleOccurrenceId = await FindEligibleOccurrenceIdAsync(req.EventId, myUserId);
        if (eligibleOccurrenceId == null)
            return StatusCode(403, new { message = "Only customers who attended a completed event occurrence can review it once." });

        var entity = new review
        {
            occurrence_id = eligibleOccurrenceId.Value,
            customer_id = myUserId,
            rating = req.Rating,
            comment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim(),
            created_at = DateTime.UtcNow
        };

        _db.reviews.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "You have already reviewed all completed occurrences you attended for this event." });
        }

        var customer = await _db.users
            .Where(u => u.user_id == myUserId)
            .Select(u => new { u.user_id, u.name })
            .FirstAsync();

        var resp = new ReviewResponse
        {
            ReviewId = entity.review_id,
            OccurrenceId = entity.occurrence_id,
            EventId = req.EventId,
            CustomerId = customer.user_id,
            CustomerName = customer.name,
            Rating = entity.rating,
            Comment = entity.comment,
            CreatedAt = entity.created_at,
            Replies = new List<ReplyResponse>()
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.review_id }, resp);
    }

    // GET /api/reviews/eligibility/{eventId}
    // Returns whether CURRENT authenticated customer can add a review.
    [HttpGet("eligibility/{eventId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ReviewEligibilityResponse>> GetEligibility(int eventId)
    {
        var evtExists = await _db.events.AnyAsync(e => e.event_id == eventId);
        if (!evtExists)
            return NotFound(new { message = "Event not found." });

        var eligibleOccurrenceId = await FindEligibleOccurrenceIdAsync(eventId, _me.UserId);

        return Ok(new ReviewEligibilityResponse
        {
            CanAddReview = eligibleOccurrenceId != null,
            EligibleOccurrenceId = eligibleOccurrenceId
        });
    }

    private async Task<int?> FindEligibleOccurrenceIdAsync(int eventId, int customerId)
    {
        return await (
            from occ in _db.event_occurrences
            join b in _db.bookings on occ.occurrence_id equals b.occurrence_id
            where occ.event_id == eventId
                  && occ.status == "Completed"
                  && b.customer_id == customerId
                  && b.status == "Confirmed"
                  && !_db.reviews.Any(r => r.occurrence_id == occ.occurrence_id && r.customer_id == customerId)
            orderby occ.date descending, occ.time descending, occ.occurrence_id descending
            select (int?)occ.occurrence_id
        ).FirstOrDefaultAsync();
    }

    // =========================
    // 2) GET /api/reviews/{id}
    // Public: Review + replies
    // =========================
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewResponse>> GetById(int id)
    {
        var r = await _db.reviews
            .AsNoTracking()
            .Where(x => x.review_id == id)
            .Select(x => new ReviewResponse
            {
                ReviewId = x.review_id,
                OccurrenceId = x.occurrence_id,
                EventId = x.occurrence.event_id,
                CustomerId = x.customer_id,
                CustomerName = x.customer.name,
                Rating = x.rating,
                Comment = x.comment,
                CreatedAt = x.created_at,
                Replies = x.replies
                    .OrderBy(z => z.created_at)
                    .Select(z => new ReplyResponse
                    {
                        ReplyId = z.reply_id,
                        ReviewId = z.review_id,
                        OrganizerId = z.organizer_id,
                        OrganizerName = z.organizer.name,
                        ReplyText = z.reply_text,
                        CreatedAt = z.created_at
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (r == null)
            return NotFound(new { message = "Review not found." });

        return Ok(r);
    }

    // =========================
    // 3) GET /api/events/{eventId}/reviews
    // Public: All reviews for an event + replies
    // (Absolute route to match your EventsController style)
    // =========================
    [HttpGet("/api/events/{eventId:int}/reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewResponse>>> GetForEvent(int eventId)
    {
        var evtExists = await _db.events.AnyAsync(e => e.event_id == eventId);
        if (!evtExists)
            return NotFound(new { message = "Event not found." });

        var items = await _db.reviews
            .AsNoTracking()
            .Where(r => r.occurrence.event_id == eventId)
            .OrderByDescending(r => r.created_at)
            .Select(r => new ReviewResponse
            {
                ReviewId = r.review_id,
                OccurrenceId = r.occurrence_id,
                EventId = r.occurrence.event_id,
                CustomerId = r.customer_id,
                CustomerName = r.customer.name,
                Rating = r.rating,
                Comment = r.comment,
                CreatedAt = r.created_at,
                Replies = r.replies
                    .OrderBy(x => x.created_at)
                    .Select(x => new ReplyResponse
                    {
                        ReplyId = x.reply_id,
                        ReviewId = x.review_id,
                        OrganizerId = x.organizer_id,
                        OrganizerName = x.organizer.name,
                        ReplyText = x.reply_text,
                        CreatedAt = x.created_at
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(items);
    }


    // =========================
    // 5) POST /api/reviews/{reviewId}/replies
    // Organizer replies ONLY if they own the event
    // =========================
    [HttpPost("{reviewId:int}/replies")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<ActionResult<ReplyResponse>> CreateReply(int reviewId, [FromBody] ReplyCreateRequest req)
    {
        if (req == null)
            return BadRequest(new { message = "Request body is required." });

        if (string.IsNullOrWhiteSpace(req.ReplyText))
            return BadRequest(new { message = "ReplyText is required." });

        var myUserId = _me.UserId;

        // Use joins (same style as EventsController)
        var reviewRow = await (
            from r in _db.reviews
            join occ in _db.event_occurrences on r.occurrence_id equals occ.occurrence_id
            join ev in _db.events on occ.event_id equals ev.event_id
            where r.review_id == reviewId
            select new
            {
                r.review_id,
                ev.org_id
            }
        ).FirstOrDefaultAsync();

        if (reviewRow == null)
            return NotFound(new { message = "Review not found." });

        if (reviewRow.org_id != myUserId)
            return StatusCode(403, new { message = "Only the event organizer can reply to this review." });

        var entity = new reply
        {
            review_id = reviewId,
            organizer_id = myUserId,
            reply_text = req.ReplyText.Trim(),
            created_at = DateTime.UtcNow
        };

        _db.replies.Add(entity);
        await _db.SaveChangesAsync();

        var orgName = await _db.users
            .Where(u => u.user_id == myUserId)
            .Select(u => u.name)
            .FirstAsync();

        var resp = new ReplyResponse
        {
            ReplyId = entity.reply_id,
            ReviewId = entity.review_id,
            OrganizerId = entity.organizer_id,
            OrganizerName = orgName,
            ReplyText = entity.reply_text,
            CreatedAt = entity.created_at
        };

        return CreatedAtAction(nameof(GetById), new { id = reviewId }, resp);
    }
}
