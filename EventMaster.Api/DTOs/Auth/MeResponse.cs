namespace EventMaster.Api.DTOs.Auth;
/// <summary>
/// Represents the response returned when a user requests their own information, containing details about the user such as their ID, role, username, email, name, and phone number.
/// </summary>
public class MeResponse
{
    public int UserId { get; set; }
    public string Role { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
}
