using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace EventMaster.Web.Services;

public class ReviewsApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReviewsApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ReviewResponse>> GetForEventAsync(int eventId)
    {
        // note: your API defines this as absolute route: /api/events/{eventId}/reviews
        var url = $"/api/events/{eventId}/reviews";
        return await _http.GetFromJsonAsync<List<ReviewResponse>>(url) ?? new List<ReviewResponse>();
    }

    public async Task<ReviewEligibilityResponse?> GetEligibilityAsync(int eventId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/reviews/eligibility/{eventId}");

        var jwt = _httpContextAccessor.HttpContext?.User.FindFirstValue("access_token");
        if (!string.IsNullOrWhiteSpace(jwt))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        var response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            return new ReviewEligibilityResponse { CanAddReview = false };

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ReviewEligibilityResponse>();
    }
}
