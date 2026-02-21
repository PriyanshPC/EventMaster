using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Identity.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
}