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
                Status = c.BookingStatus,
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
    public async Task<IActionResult> UpdateProfile(string email, string? phone, string currentPassword, string? newPassword, string? confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            SetNotification("Current Password is required to confirm profile updates.", "danger");
            return RedirectToAction(nameof(Customer), new { tab = "settings" });
        }

        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var me = await _authApi.MeAsync(jwt);
        if (me is null)
        {
            SetNotification("Unable to load current profile details.", "danger");
            return RedirectToAction(nameof(Customer), new { tab = "settings" });
        }

        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedPhone = phone?.Trim();
        var currentPhone = me.Phone?.Trim();

        var emailChanged = !string.Equals(me.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase);
        var phoneChanged = !string.Equals(currentPhone, normalizedPhone, StringComparison.OrdinalIgnoreCase);
        var wantsPasswordChange = !string.IsNullOrWhiteSpace(newPassword) || !string.IsNullOrWhiteSpace(confirmPassword);

        if (wantsPasswordChange)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                SetNotification("Provide both New Password and Confirm Password to change password.", "danger");
                return RedirectToAction(nameof(Customer), new { tab = "settings" });
            }

            if (newPassword != confirmPassword)
            {
                SetNotification("New Password and Confirm Password must match.", "danger");
                return RedirectToAction(nameof(Customer), new { tab = "settings" });
            }
        }

        ApiOperationResult? profileResult = null;
        if (emailChanged || phoneChanged)
        {
            profileResult = await _authApi.UpdateProfileAsync(new UpdateProfileRequest
            {
                CurrentPassword = currentPassword,
                Email = normalizedEmail,
                Phone = normalizedPhone
            }, jwt);

            if (!profileResult.Success)
            {
                SetNotification(profileResult.Message ?? "Unable to update profile.", "danger");
                return RedirectToAction(nameof(Customer), new { tab = "settings" });
            }
        }

        if (wantsPasswordChange)
        {
            var passwordResult = await _authApi.ChangePasswordAsync(new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword!
            }, jwt);

            if (!passwordResult.Success)
            {
                if (emailChanged || phoneChanged)
                {
                    var rollbackResult = await _authApi.UpdateProfileAsync(new UpdateProfileRequest
                    {
                        CurrentPassword = currentPassword,
                        Email = me.Email,
                        Phone = me.Phone
                    }, jwt);

                    if (rollbackResult.Success)
                    {
                        SetNotification(passwordResult.Message ?? "Unable to update password. Profile changes were rolled back.", "danger");
                    }
                    else
                    {
                        var rollbackMessage = rollbackResult.Message ?? "Unable to roll back profile changes.";
                        SetNotification($"{(passwordResult.Message ?? "Unable to update password.")} {rollbackMessage}", "danger");
                    }
                }
                else
                {
                    SetNotification(passwordResult.Message ?? "Unable to update password.", "danger");
                }

                return RedirectToAction(nameof(Customer), new { tab = "settings" });
            }

            SetNotification(passwordResult.Message ?? "Profile and password updated successfully.", "success");
            return RedirectToAction(nameof(Customer), new { tab = "settings" });
        }

        if (profileResult is not null)
        {
            SetNotification(profileResult.Message ?? "Profile updated successfully.", "success");
        }
        else
        {
            SetNotification("No changes were detected.", "info");
        }

        return RedirectToAction(nameof(Customer), new { tab = "settings" });
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
