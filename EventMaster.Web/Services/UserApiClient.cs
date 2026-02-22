using EventMaster.Web.Models.Dashboard;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace EventMaster.Web.Services;

public class DashboardService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardService(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<MeDto> GetMeAsync()
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Get, "/api/auth/me");
        var resp = await _http.SendAsync(msg);
        await EnsureSuccessAsync(resp);
        return (await resp.Content.ReadFromJsonAsync<MeDto>())!;
    }

    public async Task<List<DashboardBookingCardDto>> GetDashboardBookingsAsync()
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Get, "/api/bookings/dashboard");
        var resp = await _http.SendAsync(msg);
        await EnsureSuccessAsync(resp);
        return (await resp.Content.ReadFromJsonAsync<List<DashboardBookingCardDto>>()) ?? new List<DashboardBookingCardDto>();
    }

    public async Task<BookingDetailsDto> GetBookingAsync(int bookingId)
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Get, $"/api/bookings/{bookingId}");
        var resp = await _http.SendAsync(msg);
        await EnsureSuccessAsync(resp);
        return (await resp.Content.ReadFromJsonAsync<BookingDetailsDto>())!;
    }

    public async Task<PaymentSummaryDto?> GetPaymentByBookingAsync(int bookingId)
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Get, $"/api/payment/booking/{bookingId}");
        var resp = await _http.SendAsync(msg);

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<PaymentSummaryDto?>();
    }


    public async Task UpdateEmailAsync(string email, string currentPassword)
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Patch, "/api/auth/profile");
        msg.Content = JsonContent.Create(new UpdateProfileRequest { Email = email, CurrentPassword = currentPassword });
        var resp = await _http.SendAsync(msg);
        await EnsureSuccessAsync(resp);
    }

    public async Task UpdatePhoneAsync(string phone, string currentPassword)
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Patch, "/api/auth/profile");
        msg.Content = JsonContent.Create(new UpdateProfileRequest { Phone = phone, CurrentPassword = currentPassword });
        var resp = await _http.SendAsync(msg);
        await EnsureSuccessAsync(resp);
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Post, "/api/auth/change-password");
        msg.Content = JsonContent.Create(new ChangePasswordRequest { CurrentPassword = currentPassword, NewPassword = newPassword });
        var resp = await _http.SendAsync(msg);
        await EnsureSuccessAsync(resp);
    }

    public async Task CancelAndRefundAsync(int bookingId)
    {
        using var msg = CreateAuthorizedRequest(HttpMethod.Post, $"/api/bookings/{bookingId}/cancel-refund");
        var resp = await _http.SendAsync(msg);
        await EnsureSuccessAsync(resp);
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
    {
        var jwt = _httpContextAccessor.HttpContext?.User?.FindFirstValue("access_token");
        if (string.IsNullOrWhiteSpace(jwt))
            throw new InvalidOperationException("User session token not found.");

        var msg = new HttpRequestMessage(method, url);
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return msg;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;

        var text = await resp.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(text))
            resp.EnsureSuccessStatusCode();

        throw new InvalidOperationException(text);
    }
}
