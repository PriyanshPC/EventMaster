using EventMaster.Web.Models;
using EventMaster.Web.Services;
using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaster.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly BookingsApiClient _bookingsApi;
    private readonly AuthApiClient _authApi;
    private readonly IConfiguration _config;

    public DashboardController(BookingsApiClient bookingsApi, AuthApiClient authApi, IConfiguration config)
    {
        _bookingsApi = bookingsApi;
        _authApi = authApi;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Customer(string tab = "bookings", string? edit = null, string? message = null, string? error = null)
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
                Status = c.Status,
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
                Phone = me.Phone,
                EditMode = edit ?? ""
            },
            UpcomingBookings = mapped.Where(x => x.StartDateTimeUtc >= now).OrderBy(x => x.StartDateTimeUtc).Select(x => x.Card).ToList(),
            PastBookings = mapped.Where(x => x.StartDateTimeUtc < now).OrderByDescending(x => x.StartDateTimeUtc).Select(x => x.Card).ToList()
        };

        return View("User", vm);
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
            Status = details.Status,
            DateTimeLine = $"{details.Date:MMM d, yyyy} • {DateTime.Today.Add(details.Time.ToTimeSpan()):hh:mm tt}",
            VenueLine = $"{details.VenueName}, {details.VenueAddress}, {details.VenueCity}, {details.VenueProvince}",
            NumberOfTickets = details.NumberOfTickets,
            SeatsSelected = string.IsNullOrWhiteSpace(details.SeatsSelected) ? "General Admission" : details.SeatsSelected,
            CardSummary = details.CardSummary ?? "N/A",
            TotalAmount = details.TotalAmount,
            TicketNumber = details.TicketNumber,
            CanCancel = details.Status == "Scheduled" && details.Date.ToDateTime(details.Time) - DateTime.UtcNow >= TimeSpan.FromHours(24)
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
        return RedirectToAction(nameof(BookingDetails), new { bookingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEmail(string email, string currentPassword)
    {
        return await UpdateProfileInternal(new UpdateProfileRequest { Email = email?.Trim(), CurrentPassword = currentPassword }, "email");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePhone(string phone, string currentPassword)
    {
        return await UpdateProfileInternal(new UpdateProfileRequest { Phone = phone?.Trim(), CurrentPassword = currentPassword }, "phone");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            SetNotification("New Password and Confirm Password must match.", "danger");
            return RedirectToAction(nameof(Customer), new { tab = "settings", edit = "password" });
        }

        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var ok = await _authApi.ChangePasswordAsync(new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        }, jwt);

        SetNotification(ok ? "Password updated successfully." : "Unable to update password.", ok ? "success" : "danger");
        return RedirectToAction(nameof(Customer), new { tab = "settings" });
    }

    private async Task<IActionResult> UpdateProfileInternal(UpdateProfileRequest req, string mode)
    {
        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var ok = await _authApi.UpdateProfileAsync(req, jwt);
        SetNotification(ok ? "Profile updated successfully." : "Unable to update profile.", ok ? "success" : "danger");

        return RedirectToAction(nameof(Customer), new
        {
            tab = "settings",
            edit = ok ? null : mode
        });
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
