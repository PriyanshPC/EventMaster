using EventMaster.Web.Services.ApiDtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EventMaster.Web.Services;

public class BookingsApiClient
{
    private readonly HttpClient _http;

    public BookingsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<DashboardBookingCardDto>> GetDashboardBookingsAsync(string jwt)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/bookings/dashboard");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return new List<DashboardBookingCardDto>();
        return await resp.Content.ReadFromJsonAsync<List<DashboardBookingCardDto>>() ?? new List<DashboardBookingCardDto>();
    }

    public async Task<BookingDetailsDto?> GetBookingDetailsAsync(int bookingId, string jwt)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"api/bookings/{bookingId}/details");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<BookingDetailsDto>();
    }

    public async Task<CancelRefundResponseDto?> CancelAndRefundAsync(int bookingId, string jwt)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, $"api/bookings/{bookingId}/cancel-refund");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        req.Content = JsonContent.Create(new { });

        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<CancelRefundResponseDto>();
    }

    public async Task<List<VenueResponseDto>> GetVenuesAsync()
    {
        return await _http.GetFromJsonAsync<List<VenueResponseDto>>("api/venues") ?? new();
    }
}
