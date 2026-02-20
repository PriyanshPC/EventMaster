namespace EventMaster.Api.DTOs.Auth;

public class LoginRequest
{
    public string UsernameOrEmail { get; set; } = "";
    public string Password { get; set; } = "";
}
