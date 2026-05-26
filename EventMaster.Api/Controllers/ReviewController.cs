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
/// <summary>
/// Controller for managing event reviews and organizer replies. Customers can create reviews for events they attended,
/// and organizers can reply to reviews for their events. All users can view reviews and replies. The controller includes endpoints for creating reviews, checking review eligibility, retrieving reviews by ID or by event, and creating organizer replies. Authorization is enforced to ensure only eligible customers can create reviews and only event organizers can reply to reviews for their events.
/// </summary>
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
    // Customer can review ONLY if they attended any completed occurrence of the event
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
            return StatusCode(403, new { message = "Only customers who attended a completed event occurrence can review." });

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
            return Conflict(new { message = "Review could not be created due to a data conflict." });
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

/// <summary>
/// GET /api/reviews/eligibility/{eventId}
/// Checks if the currently authenticated customer is eligible to add a review for the specified event. Eligibility requires that the customer has a confirmed booking for an occurrence of the event that has been completed. Returns a boolean indicating eligibility and, if eligible, the ID of the occurrence they can review. This allows the frontend to determine whether to show the "Add Review" option and which occurrence the review will be associated with. Only customers can access this endpoint, and it returns 403 if a non-customer tries to access it. It returns 404 if the event does not exist.
/// </summary>
/// <param name="eventId"></param>
/// <returns></returns>
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

/// <summary>
/// Helper method to find an eligible occurrence ID for a given event and customer. An occurrence is eligible if it belongs to the specified event, has a status of "Completed", and the customer has a confirmed booking for that occurrence. The method returns the most recent eligible occurrence ID if multiple are found, or null if none are found. This is used to determine if a customer can add a review for an event and which occurrence the review should be associated with. The method uses joins to efficiently query the database and ensure all conditions are met in a single query.
/// </summary>
/// <param name="eventId"></param>
/// <param name="customerId"></param>
/// <returns></returns>
    private async Task<int?> FindEligibleOccurrenceIdAsync(int eventId, int customerId)
    {
        return await (
            from occ in _db.event_occurrences
            join b in _db.bookings on occ.occurrence_id equals b.occurrence_id
            where occ.event_id == eventId
                  && occ.status == "Completed"
                  && b.customer_id == customerId
                  && b.status == "Confirmed"
            orderby occ.date descending, occ.time descending, occ.occurrence_id descending
            select (int?)occ.occurrence_id
        ).FirstOrDefaultAsync();
    }

/// <summary>
/// GET /api/reviews/{id}
/// Retrieves a review by its ID, including any organizer replies. This endpoint is public and does not require authentication, allowing anyone to view reviews. The response includes the review details (rating, comment, customer info) and a list of replies from the organizer, ordered by creation time. If the review with the specified ID does not exist, it returns a 404 error. This allows users to read specific reviews and see if the organizer has responded to any feedback.
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
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
                    .ThenBy(z => z.reply_id)
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
            .ThenByDescending(r => r.review_id)
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
                    .ThenBy(x => x.reply_id)
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

/// <summary>
/// GET /api/reviews/mine/pending-replies
/// Retrieves a list of reviews for the authenticated organizer's events that do not yet have a reply. This allows organizers to easily find reviews that they may want to respond to. The endpoint returns the review details along with the associated event information and customer name. Only users with the "ORGANIZER" role can access this endpoint, and it will return a 403 error for any other roles. This helps organizers stay engaged with their customers by showing them which reviews still need attention.
/// The reviews are ordered by creation time, with the most recent reviews appearing first. This way, organizers can prioritize responding to newer reviews. If the organizer has no pending reviews, it returns an empty list. This endpoint is designed to improve the organizer's ability to manage customer feedback effectively.
/// </summary>
/// <returns></returns>

    [HttpGet("mine/pending-replies")]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<ActionResult<List<object>>> GetPendingReplies()
    {
        var myUserId = _me.UserId;

        var rows = await (
            from r in _db.reviews
            join occ in _db.event_occurrences on r.occurrence_id equals occ.occurrence_id
            join ev in _db.events on occ.event_id equals ev.event_id
            where ev.org_id == myUserId
                  && !r.replies.Any()
            orderby r.created_at descending, r.review_id descending
            select new
            {
                reviewId = r.review_id,
                eventId = ev.event_id,
                eventName = ev.name,
                customerName = r.customer.name,
                rating = r.rating,
                comment = r.comment,
                createdAt = r.created_at
            }
        ).ToListAsync();

        return Ok(rows);
    }

/// <summary>
/// POST /api/reviews/{reviewId}/replies
/// Allows an event organizer to reply to a review for their event. The organizer can only reply
/// if they own the event associated with the review. The request body must include the reply text. If the review already has a reply, it returns a 409 conflict error. If the review or event does not exist, it returns a 404 error. If the authenticated user is not the organizer of the event, it returns a 403 error. On success, it creates the reply and returns the details of the newly created reply in the response. This endpoint helps organizers engage with their customers by allowing them to respond to feedback on their events.
/// The reply is associated with the review and includes the organizer's name and the time it was
/// created. This allows customers to see that the organizer has acknowledged their review and provided a response, which can improve customer satisfaction and trust in the platform. By enforcing that only the event organizer can reply to reviews for their events, we ensure that responses are relevant and come from the appropriate party.
/// </summary>
/// <param name="reviewId"></param>
/// <param name="req"></param>
/// <returns></returns>
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

        var hasReply = await _db.replies.AnyAsync(x => x.review_id == reviewId);
        if (hasReply)
            return Conflict(new { message = "This review already has a reply." });

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
