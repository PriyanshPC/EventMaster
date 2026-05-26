using System.Security.Claims;

namespace EventMaster.Api.Security;
/// <summary>
/// Represents the currently authenticated user in the application, providing properties to access user information such as authentication status, user ID, role, username, and email.
/// This class relies on the IHttpContextAccessor to access the current HTTP context and extract user claims
/// to populate its properties. It provides a convenient way to access user information throughout the application without needing to directly interact with the HTTP context in multiple places, promoting cleaner and more maintainable code.
/// The properties include:
/// - IsAuthenticated: Indicates whether the user is authenticated.
/// - UserId: Retrieves the user's ID from the claims, returning 0 if the claim is missing or cannot be parsed as an integer.
/// - Role: Retrieves the user's role from the claims, returning null if the claim is missing.
/// - Username: Retrieves the user's username from the claims, returning null if the claim is missing.
/// - Email: Retrieves the user's email from the claims, returning null if the claim is missing.
/// This class is typically used in scenarios where you need to access the current user's information in services, controllers, or other parts of the application without directly coupling those components to the HTTP context, allowing for better separation of concerns and easier testing.
/// </summary>
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
