namespace EventMaster.Api.DTOs.Auth;

public class MeResponse
{
    public int UserId { get; set; }
    public string Role { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
}
