namespace EventMaster.Api.DTOs.Auth;

public class UpdateProfileRequest
{
    public string CurrentPassword { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
