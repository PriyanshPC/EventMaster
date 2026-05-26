namespace EventMaster.Api.DTOs.Auth;
/// <summary>
/// Represents the request to register a new user, containing details such as name, age, phone number, email, username, and password.
/// </summary>
public class RegisterRequest
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
