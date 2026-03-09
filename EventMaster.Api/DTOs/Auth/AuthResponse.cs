namespace EventMaster.Api.DTOs.Auth;
/// <summary>
/// Represents the response returned after a successful authentication, containing the JWT token.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = "";
}
