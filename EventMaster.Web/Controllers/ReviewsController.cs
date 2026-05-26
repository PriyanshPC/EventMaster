using EventMaster.Web.Models;
using EventMaster.Web.Services;
using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaster.Web.Controllers;

[Authorize]
/// <summary>
/// Handles user reviews for events, including displaying the review submission form and processing review submissions. Integr
/// ates with the Reviews API to check eligibility for submitting a review (only customers who have attended a completed occurrence can review) and to submit new reviews. Also retrieves existing reviews for display on the review submission page.
/// The Create GET action checks if the user is eligible to submit a review for the specified event
/// occurrence, and if so, retrieves the occurrence details and existing reviews to build a view model for the review submission page. If the user is not eligible, they are redirected back to the event details page with a notification.
/// The Create POST action validates the user's eligibility again, then submits the review to the Reviews API
/// using the JWT from the user's claims for authentication. It provides feedback on the success or failure of the review submission and redirects appropriately.
/// The SetNotification method is a helper to set TempData for displaying notifications to the user.
/// This controller ensures that only authorized customers can submit reviews and that they can only review events they have attended, maintaining the integrity of the review system. It also provides a seamless user experience by integrating with the Events API to show relevant event information on the review page.
/// </summary>
public class ReviewsController : Controller
{
    private readonly EventsApiClient _eventsApi;
    private readonly ReviewsApiClient _reviewsApi;
    private readonly IConfiguration _config;

    public ReviewsController(EventsApiClient eventsApi, ReviewsApiClient reviewsApi, IConfiguration config)
    {
        _eventsApi = eventsApi;
        _reviewsApi = reviewsApi;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int eventId, int occurrenceId)
    {
        if (!User.IsInRole("CUSTOMER"))
        {
            SetNotification("Only customers can submit reviews.", "warning");
            return RedirectToAction("Index", "Events");
        }

        var eligibility = await _reviewsApi.GetEligibilityAsync(eventId);
        if (eligibility?.CanAddReview != true)
        {
            SetNotification("You can review only after attending a completed occurrence.", "warning");
            return RedirectToAction("Details", "Events", new { eventId, occurrenceId });
        }

        var details = await _eventsApi.GetOccurrenceDetailsAsync(eventId, occurrenceId);
        if (details is null) return NotFound();

        var reviews = await _reviewsApi.GetForEventAsync(eventId);
        return View(BuildVm(eventId, occurrenceId, details, reviews));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int eventId, int occurrenceId, ReviewFormInput input)
    {
        if (!User.IsInRole("CUSTOMER")) return Forbid();

        var eligibility = await _reviewsApi.GetEligibilityAsync(eventId);
        if (eligibility?.CanAddReview != true)
        {
            SetNotification("You can review only after attending a completed occurrence.", "warning");
            return RedirectToAction("Details", "Events", new { eventId, occurrenceId });
        }

        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var result = await _reviewsApi.SubmitReviewAsync(new ReviewCreateRequestDto
        {
            EventId = eventId,
            Rating = input.Rating,
            Comment = input.Comment
        }, jwt);

        if (!result.Success)
        {
            SetNotification(result.ErrorMessage ?? "Unable to submit review.", "danger");
            return RedirectToAction(nameof(Create), new { eventId, occurrenceId });
        }

        SetNotification("Review submitted.", "success");
        return RedirectToAction("Details", "Events", new { eventId, occurrenceId });
    }

    private ReviewCreatePageViewModel BuildVm(int eventId, int occurrenceId, EventOccurrenceDetailsResponse details, List<ReviewResponse> reviews)
    {
        var apiBase = (_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');
        return new ReviewCreatePageViewModel
        {
            EventId = eventId,
            OccurrenceId = occurrenceId,
            Details = details,
            Reviews = reviews.OrderByDescending(r => r.CreatedAt).ToList(),
            HeaderWhen = $"{details.Date:MMM d, yyyy} • {DateTime.Today.Add(details.Time):hh:mm tt}",
            HeaderWhere = $"{details.VenueName} • {details.City}, {details.Province}",
            ImageUrl = $"{apiBase}/api/events/{eventId}/image"
        };
    }

    private void SetNotification(string message, string type = "info", int delayMilliseconds = 5000)
    {
        TempData["Notification.Message"] = message;
        TempData["Notification.Type"] = type;
        TempData["Notification.Delay"] = delayMilliseconds;
    }
}
