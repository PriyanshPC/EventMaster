using System.Security.Claims;

namespace EventMaster.Api.Security;

public class CurrentUser
{
    private readonly IHttpContextAccessor _http;

    public CurrentUser(IHttpContextAccessor http)
    {
        _http = http;
    }

    public bool IsAuthenticated =>
        _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public int UserId
    {
        get
        {
            var raw = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : 0;
        }
    }

    // DB-first: keep Role as string (because your DB column is enum/string)
    public string? Role =>
        _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public string? Username =>
        _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public string? Email =>
        _http.HttpContext?.User?.FindFirstValue("email");
}
