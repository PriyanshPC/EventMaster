using EventMaster.Web.Services.ApiDtos;
using System.Net.Http.Json;

namespace EventMaster.Web.Services;

public class ReviewsApiClient
{
    private readonly HttpClient _http;

    public ReviewsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ReviewResponse>> GetForEventAsync(int eventId)
    {
        // note: your API defines this as absolute route: /api/events/{eventId}/reviews
        var url = $"/api/events/{eventId}/reviews";
        return await _http.GetFromJsonAsync<List<ReviewResponse>>(url) ?? new List<ReviewResponse>();
    }
}