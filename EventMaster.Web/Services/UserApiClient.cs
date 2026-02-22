using EventMaster.Web.Models.Dashboard;

namespace EventMaster.Web.Services;

public class DashboardService
{
    // Replace this with whatever your repo uses (ApiClient, ApiService, etc.)
    private readonly IApiClient _api;

    public DashboardService(IApiClient api)
    {
        _api = api;
    }

    public Task<MeDto> GetMeAsync()
        => _api.GetAsync<MeDto>("/api/auth/me");

    public Task<List<DashboardBookingCardDto>> GetDashboardBookingsAsync()
        => _api.GetAsync<List<DashboardBookingCardDto>>("/api/bookings/dashboard");

    public Task<BookingDetailsDto> GetBookingAsync(int bookingId)
        => _api.GetAsync<BookingDetailsDto>($"/api/bookings/{bookingId}");

    public Task<PaymentSummaryDto?> GetPaymentByBookingAsync(int bookingId)
        => _api.GetAsync<PaymentSummaryDto?>($"/api/payment/booking/{bookingId}");

    public Task CancelAndRefundAsync(int bookingId)
        => _api.PostAsync($"/api/bookings/{bookingId}/cancel-refund", body: null);
}