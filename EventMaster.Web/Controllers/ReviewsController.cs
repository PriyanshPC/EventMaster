using EventMaster.Web.Models;
using EventMaster.Web.Services;
using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaster.Web.Controllers;

[Authorize]
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
