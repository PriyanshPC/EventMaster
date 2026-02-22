using EventMaster.Web.Models;
using EventMaster.Web.Services;
using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace EventMaster.Web.Controllers;

public class BookingsController : Controller
{
    private const string SessionIntentKey = "Booking.Intent";
    private readonly EventsApiClient _eventsApi;
    private readonly PaymentsApiClient _paymentsApi;
    private readonly IConfiguration _config;

    public BookingsController(EventsApiClient eventsApi, PaymentsApiClient paymentsApi, IConfiguration config)
    {
        _eventsApi = eventsApi;
        _paymentsApi = paymentsApi;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int eventId, int occurrenceId)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            SetNotification("Please login to book tickets.", "warning");
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Create), new { eventId, occurrenceId }) });
        }

        var details = await _eventsApi.GetOccurrenceDetailsAsync(eventId, occurrenceId);
        if (details is null) return NotFound();

        var input = new BookingFormInput();
        var intent = GetIntent();
        if (intent?.OccurrenceId == occurrenceId)
        {
            input.Quantity = intent.Quantity;
            input.Seats = intent.Seats;
        }

        return View("Booking", BuildBookingVm(details, input));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int eventId, int occurrenceId, BookingFormInput input)
    {
        var details = await _eventsApi.GetOccurrenceDetailsAsync(eventId, occurrenceId);
        if (details is null) return NotFound();

        input.Quantity = Math.Clamp(input.Quantity, 1, 10);
        input.Seats = (input.Seats ?? new()).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToList();
        var occupied = ParseSeats(details.SeatsOccupied);

        if (details.Seating)
        {
            if (input.Seats.Count != input.Quantity)
                ModelState.AddModelError("Input.Seats", "Select seats matching ticket quantity.");
            if (input.Seats.Any(s => occupied.Contains(s)))
                ModelState.AddModelError("Input.Seats", "One or more selected seats are unavailable.");
        }
        else
        {
            input.Seats = new();
        }

        if (!ModelState.IsValid)
            return View("Booking", BuildBookingVm(details, input));

        SetIntent(new BookingIntentModel
        {
            EventId = eventId,
            OccurrenceId = occurrenceId,
            Quantity = input.Quantity,
            Seats = input.Seats
        });

        return RedirectToAction(nameof(Payment));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Payment()
    {
        var intent = GetIntent();
        if (intent is null) return RedirectToAction("Index", "Events");

        var details = await _eventsApi.GetOccurrenceDetailsAsync(intent.EventId, intent.OccurrenceId);
        if (details is null) return NotFound();

        return View(BuildPaymentVm(intent, details));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(PaymentFormInput input)
    {
        var intent = GetIntent();
        if (intent is null) return RedirectToAction("Index", "Events");

        var details = await _eventsApi.GetOccurrenceDetailsAsync(intent.EventId, intent.OccurrenceId);
        if (details is null) return NotFound();

        if (!ModelState.IsValid)
            return View(BuildPaymentVm(intent, details, input));

        var jwt = User.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return RedirectToAction("Login", "Account");

        var paymentResult = await _paymentsApi.FinalizeBookingAsync(new PaymentCreateRequestDto
        {
            OccurrenceId = intent.OccurrenceId,
            Quantity = intent.Quantity,
            Seats = intent.Seats,
            CouponCode = input.DiscountCoupon,
            NameOnCard = input.NameOnCard,
            CardNumber = input.CardNumber,
            Exp = input.Exp,
            Cvv = input.Cvv,
            PostalCode = input.PostalCode
        }, jwt);

        if (!paymentResult.Success)
        {
            var type = paymentResult.StatusCode == 409 ? "warning" : "danger";
            SetNotification(paymentResult.ErrorMessage ?? "Payment failed.", type);
            return RedirectToAction(nameof(Payment));
        }

        ClearIntent();
        SetNotification("Payment successful.", "success");
        return RedirectToAction("BookingDetails", "Dashboard", new { bookingId = paymentResult.Response!.BookingId });
    }

    private BookingPageViewModel BuildBookingVm(EventOccurrenceDetailsResponse details, BookingFormInput input)
    {
        var apiBase = (_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');
        return new BookingPageViewModel
        {
            EventId = details.EventId,
            OccurrenceId = details.OccurrenceId,
            Details = details,
            HeaderWhen = $"{details.Date:MMM d, yyyy} • {DateTime.Today.Add(details.Time):hh:mm tt}",
            HeaderWhere = $"{details.VenueName} • {details.City}, {details.Province}",
            ImageUrl = $"{apiBase}/api/events/{details.EventId}/image",
            TotalAmount = details.Price * input.Quantity,
            RequestedSeats = input.Seats,
            OccupiedSeats = ParseSeats(details.SeatsOccupied),
            Input = input
        };
    }

    private PaymentPageViewModel BuildPaymentVm(BookingIntentModel intent, EventOccurrenceDetailsResponse details, PaymentFormInput? input = null)
    {
        var apiBase = (_config["Api:PublicBaseUrl"] ?? _config["Api:BaseUrl"] ?? "http://127.0.0.1:8081/").TrimEnd('/');
        return new PaymentPageViewModel
        {
            Intent = intent,
            Details = details,
            HeaderWhen = $"{details.Date:MMM d, yyyy} • {DateTime.Today.Add(details.Time):hh:mm tt}",
            HeaderWhere = $"{details.VenueName} • {details.City}, {details.Province}",
            ImageUrl = $"{apiBase}/api/events/{details.EventId}/image",
            TotalAmount = details.Price * intent.Quantity,
            Input = input ?? new PaymentFormInput()
        };
    }

    private static HashSet<string> ParseSeats(string? seats)
    {
        return seats?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new(StringComparer.OrdinalIgnoreCase);
    }

    private BookingIntentModel? GetIntent()
    {
        var raw = HttpContext.Session.GetString(SessionIntentKey);
        return string.IsNullOrWhiteSpace(raw) ? null : JsonSerializer.Deserialize<BookingIntentModel>(raw);
    }

    private void SetIntent(BookingIntentModel model) => HttpContext.Session.SetString(SessionIntentKey, JsonSerializer.Serialize(model));
    private void ClearIntent() => HttpContext.Session.Remove(SessionIntentKey);

    private void SetNotification(string message, string type = "info", int delayMilliseconds = 5000)
    {
        TempData["Notification.Message"] = message;
        TempData["Notification.Type"] = type;
        TempData["Notification.Delay"] = delayMilliseconds;
    }
}
