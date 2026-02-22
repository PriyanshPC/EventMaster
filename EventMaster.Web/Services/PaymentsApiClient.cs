using EventMaster.Web.Services.ApiDtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EventMaster.Web.Services;

public class PaymentsApiClient
{
    private readonly HttpClient _http;

    public PaymentsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool Success, PaymentResponseDto? Response, string? ErrorMessage)> CreatePaymentAsync(PaymentCreateRequestDto payload, string jwt)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "api/payment");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        req.Content = JsonContent.Create(payload);

        var resp = await _http.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();
        PaymentResponseDto? data = null;
        try { data = JsonSerializer.Deserialize<PaymentResponseDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); } catch { }

        if (resp.IsSuccessStatusCode)
        {
            return (true, data, null);
        }

        string? message = data?.Details;
        if (string.IsNullOrWhiteSpace(message))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var m)) message = m.GetString();
                else if (doc.RootElement.TryGetProperty("details", out var d)) message = d.GetString();
            }
            catch { }
        }

        return (false, data, message ?? "Payment failed.");
    }
}
