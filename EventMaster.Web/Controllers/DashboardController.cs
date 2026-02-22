using EventMaster.Web.Models;
using EventMaster.Web.Services;
using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace EventMaster.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly BookingsApiClient _bookingsApi;
    private readonly AuthApiClient _authApi;
    private readonly EventsApiClient _eventsApi;
    private readonly ReviewsApiClient _reviewsApi;
    private readonly IConfiguration _config;
    private static readonly string[] AllowedCategories = ["Comedy", "Parties", "Sports", "Concepts", "Theater"];

    public DashboardController(BookingsApiClient bookingsApi, AuthApiClient authApi, EventsApiClient eventsApi, ReviewsApiClient reviewsApi, IConfiguration config)
    {
        _bookingsApi = bookingsApi;
        _authApi = authApi;
        _eventsApi = eventsApi;
        _reviewsApi = reviewsApi;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Customer(string tab = "bookings", string? message = null, string? error = null)
    {
        SetNotificationFromLegacyParams(message, error);

        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var me = await _authApi.MeAsync(jwt);
        if (me is null) return RedirectToAction("Login", "Account");

        var cards = await _bookingsApi.GetDashboardBookingsAsync(jwt);
        var now = DateTime.UtcNow;
        var apiBase = (_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');

        var mapped = cards.Select(c => new
        {
            Card = new DashboardBookingCardViewModel
            {
                BookingId = c.BookingId,
                EventId = c.EventId,
                OccurrenceId = c.OccurrenceId,
                Name = c.Name,
                Category = c.Category,
                Status = (c.BookingStatus == "Confirmed" && c.Status == "Completed") ? c.Status : c.BookingStatus,
                SubTitle = $"{c.Date:MMM d} • {DateTime.Today.Add(c.Time.ToTimeSpan()):hh:mm tt}",
                VenueLine = $"{c.VenueName} • {c.VenueCity}",
                ImageUrl = $"{apiBase}/api/events/{c.EventId}/image"
            },
            c.StartDateTimeUtc
        }).ToList();

        var vm = new CustomerDashboardViewModel
        {
            ActiveTab = tab,
            Settings = new MeSettingsViewModel
            {
                Name = me.Name,
                Username = me.Username,
                Email = me.Email,
                Phone = me.Phone
            },
            UpcomingBookings = mapped.Where(x => x.StartDateTimeUtc >= now).OrderBy(x => x.StartDateTimeUtc).Select(x => x.Card).ToList(),
            PastBookings = mapped.Where(x => x.StartDateTimeUtc < now).OrderByDescending(x => x.StartDateTimeUtc).Select(x => x.Card).ToList()
        };

        return View("User", vm);
    }

    [HttpGet]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> Organizer(string tab = "events")
    {
        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var me = await _authApi.MeAsync(jwt);
        if (me is null) return RedirectToAction("Login", "Account");

        var myEvents = await _eventsApi.GetMyEventsAsync(jwt);
        var venues = await _bookingsApi.GetVenuesAsync();
        var pendingReviews = await _reviewsApi.GetPendingReviewsAsync(jwt);

        var vm = new OrganizerDashboardViewModel
        {
            ActiveTab = tab,
            Settings = new MeSettingsViewModel
            {
                Name = me.Name,
                Username = me.Username,
                Email = me.Email,
                Phone = me.Phone
            },
            EventCards = myEvents
                .SelectMany(e => e.Occurrences.Select(o => new OrganizerEventCardViewModel
                {
                    EventId = e.EventId,
                    OccurrenceId = o.OccurrenceId,
                    Name = e.Name,
                    Category = e.Category,
                    SubTitle = $"{o.Date:MMM d} • {DateTime.Today.Add(o.Time.ToTimeSpan()):hh:mm tt}",
                    VenueLine = $"{o.VenueName} • {o.VenueCity}",
                    Status = o.Status,
                    ImageUrl = $"{(_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/')}/api/events/{e.EventId}/image"
                }))
                .OrderBy(x => x.SubTitle)
                .ToList(),
            PendingReviews = pendingReviews.Select(r => new OrganizerPendingReviewViewModel
            {
                ReviewId = r.ReviewId,
                EventId = r.EventId,
                EventName = r.EventName,
                CustomerName = r.CustomerName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList(),
            AddEventForm = new OrganizerCreateEventFormViewModel
            {
                Categories = AllowedCategories.ToList(),
                Venues = venues.Select(v => new VenueOptionViewModel { VenueId = v.VenueId, Name = v.Name }).ToList()
            }
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "ORGANIZER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadEvent(string name, string category, string? description, IFormFile? image,
        List<string> occurrenceDate, List<string> occurrenceTime, List<string> occurrenceVenueId, List<string> occurrencePrice)
    {
        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var occurrences = new List<OrganizerCreateOccurrenceDto>();
        for (var i = 0; i < occurrenceDate.Count; i++)
        {
            var d = occurrenceDate[i]?.Trim();
            var t = i < occurrenceTime.Count ? occurrenceTime[i]?.Trim() : null;
            var v = i < occurrenceVenueId.Count ? occurrenceVenueId[i]?.Trim() : null;
            var p = i < occurrencePrice.Count ? occurrencePrice[i]?.Trim() : null;

            var isEmptyRow = string.IsNullOrWhiteSpace(d) && string.IsNullOrWhiteSpace(t) && string.IsNullOrWhiteSpace(v) && string.IsNullOrWhiteSpace(p);
            if (isEmptyRow)
            {
                SetNotification("No empty occurrence rows are allowed.", "danger");
                return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
            }

            if (!DateOnly.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                SetNotification("Each occurrence must have a valid date.", "danger");
                return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
            }

            if (parsedDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                SetNotification("Occurrence date cannot be in the past.", "danger");
                return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
            }

            if (!TryParseTime12HourToTimeSpan(t, out var parsedTime))
            {
                SetNotification("Each occurrence must have a valid 12-hour time.", "danger");
                return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
            }

            if (!int.TryParse(v, out var parsedVenueId) || parsedVenueId <= 0)
            {
                SetNotification("Each occurrence must have a venue.", "danger");
                return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
            }

            if (!decimal.TryParse(p, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedPrice))
            {
                SetNotification("Price must be numeric.", "danger");
                return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
            }

            occurrences.Add(new OrganizerCreateOccurrenceDto
            {
                Date = parsedDate,
                Time = parsedTime,
                VenueId = parsedVenueId,
                Price = parsedPrice
            });
        }

        if (occurrences.Count == 0)
        {
            SetNotification("Add at least one occurrence.", "danger");
            return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
        }

        await using var imageStream = image?.OpenReadStream();
        var created = await _eventsApi.CreateEventSeriesAsync(new OrganizerCreateEventSeriesRequestDto
        {
            Name = name,
            Category = category,
            Description = description,
            Occurrences = occurrences
        }, imageStream, image?.FileName, jwt);

        if (created is null)
        {
            SetNotification("Failed to upload event.", "danger");
            return RedirectToAction(nameof(Organizer), new { tab = "add-event" });
        }

        SetNotification("Event uploaded successfully.", "success");
        return RedirectToAction("Details", "Events", new { eventId = created.EventId, occurrenceId = created.OccurrenceIds.FirstOrDefault() });
    }

    [HttpPost]
    [Authorize(Roles = "ORGANIZER")]
    [ValidateAntiForgeryToken]
    public IActionResult ClearOrganizerForm() => RedirectToAction(nameof(Organizer));

    [HttpPost]
    [Authorize(Roles = "ORGANIZER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReply(int reviewId, string replyText)
    {
        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var ok = await _reviewsApi.SubmitReplyAsync(reviewId, replyText, jwt);
        SetNotification(ok ? "Reply submitted" : "Unable to submit reply.", ok ? "success" : "danger");
        return RedirectToAction(nameof(Organizer), new { tab = "reviews" });
    }

    [HttpPost]
    [Authorize(Roles = "ORGANIZER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOccurrence(int eventId, int occurrenceId)
    {
        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var ok = await _eventsApi.CancelOccurrenceAsync(eventId, occurrenceId, jwt);
        SetNotification(ok ? "Occurrence cancelled successfully." : "Unable to cancel occurrence.", ok ? "success" : "danger");
        return RedirectToAction(nameof(Organizer), new { tab = "events" });
    }

    [HttpGet]
    [Authorize(Roles = "ORGANIZER")]
    public async Task<IActionResult> OrganizerEventDetails(int eventId, int occurrenceId)
    {
        var details = await _eventsApi.GetOccurrenceDetailsAsync(eventId, occurrenceId);
        if (details is null) return NotFound();

        var time12hr = DateTime.Today.Add(details.Time).ToString("hh:mm tt");
        var headerWhen = $"{details.Date:MMM d, yyyy} • {time12hr}";
        var headerWhere = $"{details.VenueName} • {details.City}, {details.Province}";
        var apiBase = (_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');

        var vm = new OrganizerOccurrenceDetailsViewModel
        {
            EventId = eventId,
            OccurrenceId = occurrenceId,
            Details = details,
            HeaderWhen = headerWhen,
            HeaderWhere = headerWhere,
            ImageUrl = $"{apiBase}/api/events/{eventId}/image",
            CanCancel = details.Status == "Scheduled" && details.Date.ToDateTime(TimeOnly.FromTimeSpan(details.Time)) > DateTime.UtcNow
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> BookingDetails(int bookingId, string? message = null, string? error = null)
    {
        SetNotificationFromLegacyParams(message, error);

        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var apiBase = (_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');
        var details = await _bookingsApi.GetBookingDetailsAsync(bookingId, jwt);
        if (details is null) return NotFound();

        var vm = new BookingDetailsViewModel
        {
            BookingId = details.BookingId,
            EventId = details.EventId,
            OccurrenceId = details.OccurrenceId,
            EventName = details.EventName,
            Image = $"{apiBase}/api/events/{details.EventId}/image",
            Status = (details.BookingStatus == "Confirmed" && details.Status == "Completed") ? details.Status : details.BookingStatus,
            DateTimeLine = $"{details.Date:MMM d, yyyy} • {DateTime.Today.Add(details.Time.ToTimeSpan()):hh:mm tt}",
            VenueLine = $"{details.VenueName}, {details.VenueAddress}, {details.VenueCity}, {details.VenueProvince}",
            NumberOfTickets = details.NumberOfTickets,
            SeatsSelected = string.IsNullOrWhiteSpace(details.SeatsSelected) ? "General Admission" : details.SeatsSelected,
            CardSummary = details.CardSummary ?? "N/A",
            TotalAmount = details.TotalAmount,
            TicketNumber = details.TicketNumber,
            CanCancel = details.CanCancel,
            IsPastBooking = details.Date.ToDateTime(details.Time) < DateTime.UtcNow,
            ShowAddReviewButton = User.IsInRole("CUSTOMER") && details.Date.ToDateTime(details.Time) < DateTime.UtcNow,
            AddReviewUrl = $"/Reviews/Create?eventId={details.EventId}&occurrenceId={details.OccurrenceId}",
            RefundedAmount = details.RefundedAmount,
            IsRefundPending = details.IsRefundPending
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int bookingId)
    {
        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var resp = await _bookingsApi.CancelAndRefundAsync(bookingId, jwt);
        if (resp is null)
        {
            SetNotification("Unable to cancel booking.", "danger");
            return RedirectToAction(nameof(BookingDetails), new { bookingId });
        }

        SetNotification($"{resp.Message} Refunded: ${resp.RefundedAmount:0.00}", "success");
        return RedirectToAction(nameof(Customer), new { tab = "bookings" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string email, string? phone, string currentPassword, string? newPassword, string? confirmPassword, string? dashboardType)
    {
        var targetDashboard = string.Equals(dashboardType, "organizer", StringComparison.OrdinalIgnoreCase) ? nameof(Organizer) : nameof(Customer);
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            SetNotification("Current Password is required to confirm profile updates.", "danger");
            return RedirectToAction(targetDashboard, new { tab = "settings" });
        }

        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var me = await _authApi.MeAsync(jwt);
        if (me is null)
        {
            SetNotification("Unable to load current profile details.", "danger");
            return RedirectToAction(targetDashboard, new { tab = "settings" });
        }

        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedPhone = phone?.Trim();
        var emailChanged = !string.Equals(me.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase);
        var phoneChanged = !string.Equals(me.Phone?.Trim(), normalizedPhone, StringComparison.OrdinalIgnoreCase);
        var wantsPasswordChange = !string.IsNullOrWhiteSpace(newPassword) || !string.IsNullOrWhiteSpace(confirmPassword);

        var changeCount = (emailChanged ? 1 : 0) + (phoneChanged ? 1 : 0) + (wantsPasswordChange ? 1 : 0);
        if (changeCount > 1)
        {
            SetNotification("Update only one item at a time: Email, Phone, or Password.", "danger");
            return RedirectToAction(targetDashboard, new { tab = "settings" });
        }

        if (changeCount == 0)
        {
            SetNotification("No changes were detected.", "info");
            return RedirectToAction(targetDashboard, new { tab = "settings" });
        }

        if (wantsPasswordChange)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                SetNotification("Provide both New Password and Confirm Password to change password.", "danger");
                return RedirectToAction(targetDashboard, new { tab = "settings" });
            }

            if (newPassword != confirmPassword)
            {
                SetNotification("New Password and Confirm Password must match.", "danger");
                return RedirectToAction(targetDashboard, new { tab = "settings" });
            }

            var passwordResult = await _authApi.ChangePasswordAsync(new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            }, jwt);

            SetNotification(passwordResult.Message ?? (passwordResult.Success ? "Password updated successfully." : "Unable to update password."), passwordResult.Success ? "success" : "danger");
            return RedirectToAction(targetDashboard, new { tab = "settings" });
        }

        var profileResult = await _authApi.UpdateProfileAsync(new UpdateProfileRequest
        {
            CurrentPassword = currentPassword,
            Email = emailChanged ? normalizedEmail : string.Empty,
            Phone = phoneChanged ? normalizedPhone : string.Empty
        }, jwt);

        SetNotification(profileResult.Message ?? (profileResult.Success ? "Profile updated successfully." : "Unable to update profile."), profileResult.Success ? "success" : "danger");
        return RedirectToAction(targetDashboard, new { tab = "settings" });
    }

    private static bool TryParseTime12HourToTimeSpan(string? input, out TimeSpan time)
    {
        var parsed = DateTime.TryParseExact(input, new[] { "h:mm tt", "hh:mm tt" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt);
        time = parsed ? dt.TimeOfDay : TimeSpan.Zero;
        return parsed;
    }

    private void SetNotificationFromLegacyParams(string? message, string? error)
    {
        if (!string.IsNullOrWhiteSpace(message))
            SetNotification(message, "success");
        else if (!string.IsNullOrWhiteSpace(error))
            SetNotification(error, "danger");
    }

    private void SetNotification(string message, string type = "info", int delayMilliseconds = 5000)
    {
        TempData["Notification.Message"] = message;
        TempData["Notification.Type"] = type;
        TempData["Notification.Delay"] = delayMilliseconds;
    }
}
