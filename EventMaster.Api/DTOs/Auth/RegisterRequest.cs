namespace EventMaster.Api.DTOs.Auth;

public class RegisterRequest
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
