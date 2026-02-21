using EventMaster.Web.Services.ApiDtos;

namespace EventMaster.Web.Services;

public class EventsApiClient
{
    private readonly HttpClient _http;

    public EventsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<EventOccurrenceDetailsResponse?> GetOccurrenceDetailsAsync(int eventId, int occurrenceId)
    {
        var url = $"/api/events/{eventId}/occurrences/{occurrenceId}";
        return await _http.GetFromJsonAsync<EventOccurrenceDetailsResponse>(url);
    }

    public async Task<List<UpcomingEventResponse>> GetUpcomingAsync(
        string? city = null, string? q = null, string? category = null)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(city)) qs.Add($"city={Uri.EscapeDataString(city)}");
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q)}");
        if (!string.IsNullOrWhiteSpace(category)) qs.Add($"category={Uri.EscapeDataString(category)}");

        var url = "api/events/upcoming" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        return await _http.GetFromJsonAsync<List<UpcomingEventResponse>>(url)
               ?? new List<UpcomingEventResponse>();
    }
}