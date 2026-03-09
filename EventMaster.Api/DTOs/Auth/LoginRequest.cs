namespace EventMaster.Api.DTOs.Auth;
/// <summary>
/// Represents the request to log in a user, containing either the username or email and the password.
/// </summary>
public class LoginRequest
{
    public string UsernameOrEmail { get; set; } = "";
    public string Password { get; set; } = "";
}
