using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Identity.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EventMaster.Web.Services;

public class AuthApiClient
{
    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse?> LoginAsync(UserLoginRequest req)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<AuthResponse?> RegisterAsync(UserRegisterRequest req)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/register", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task<MeResponse?> MeAsync(string jwt)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var resp = await _http.SendAsync(msg);
        if (!resp.IsSuccessStatusCode) return null;

        return await resp.Content.ReadFromJsonAsync<MeResponse>();
    }

    public async Task<ApiOperationResult> UpdateProfileAsync(UpdateProfileRequest req, string jwt)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Patch, "api/auth/profile")
        {
            Content = JsonContent.Create(req)
        };
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var resp = await _http.SendAsync(msg);
        var message = await ExtractApiMessageAsync(resp);
        return new ApiOperationResult
        {
            Success = resp.IsSuccessStatusCode,
            Message = message
        };
    }

    public async Task<ApiOperationResult> ChangePasswordAsync(ChangePasswordRequest req, string jwt)
    {
        using var msg = new HttpRequestMessage(HttpMethod.Post, "api/auth/change-password")
        {
            Content = JsonContent.Create(req)
        };
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var resp = await _http.SendAsync(msg);
        var message = await ExtractApiMessageAsync(resp);
        return new ApiOperationResult
        {
            Success = resp.IsSuccessStatusCode,
            Message = message
        };
    }

    private static async Task<string?> ExtractApiMessageAsync(HttpResponseMessage resp)
    {
        if (resp.Content is null) return null;

        var body = await resp.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body)) return null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
                    return messageProp.GetString();

                if (root.TryGetProperty("error", out var errorProp) && errorProp.ValueKind == JsonValueKind.String)
                    return errorProp.GetString();

                if (root.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                    return titleProp.GetString();

                if (root.TryGetProperty("detail", out var detailProp) && detailProp.ValueKind == JsonValueKind.String)
                    return detailProp.GetString();
            }

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString();
        }
        catch (JsonException)
        {
            // Fall through and return raw response when body is not JSON.
        }

        return body.Trim();
    }
}
