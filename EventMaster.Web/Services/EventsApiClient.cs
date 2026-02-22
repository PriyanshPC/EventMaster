using EventMaster.Web.Services.ApiDtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IO;

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

    public async Task<List<OrganizerEventListItemDto>> GetMyEventsAsync(string jwt)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "api/events/mine");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return new();
        return await resp.Content.ReadFromJsonAsync<List<OrganizerEventListItemDto>>() ?? new();
    }

    public async Task<OrganizerCreatedEventSeriesResponseDto?> CreateEventSeriesAsync(OrganizerCreateEventSeriesRequestDto payload, Stream? imageStream, string? imageFileName, string jwt)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(payload.Name), "Name");
        form.Add(new StringContent(payload.Category), "Category");
        form.Add(new StringContent(payload.Description ?? string.Empty), "Description");

        for (var i = 0; i < payload.Occurrences.Count; i++)
        {
            var occ = payload.Occurrences[i];
            form.Add(new StringContent(occ.Date.ToString("yyyy-MM-dd")), $"Occurrences[{i}].Date");
            form.Add(new StringContent(occ.Time.ToString("c")), $"Occurrences[{i}].Time");
            form.Add(new StringContent(occ.VenueId.ToString()), $"Occurrences[{i}].VenueId");
            form.Add(new StringContent(occ.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), $"Occurrences[{i}].Price");
        }

        if (imageStream is not null && !string.IsNullOrWhiteSpace(imageFileName))
        {
            var imageContent = new StreamContent(imageStream);
            var ext = Path.GetExtension(imageFileName).ToLowerInvariant();
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(ext == ".png" ? "image/png" : "image/jpeg");
            form.Add(imageContent, "Image", imageFileName);
        }

        using var req = new HttpRequestMessage(HttpMethod.Post, "api/events/series") { Content = form };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<OrganizerCreatedEventSeriesResponseDto>();
    }

    public async Task<bool> CancelOccurrenceAsync(int eventId, int occurrenceId, string jwt)
    {
        using var req = new HttpRequestMessage(HttpMethod.Delete, $"api/events/{eventId}/occurrences/{occurrenceId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }
}
