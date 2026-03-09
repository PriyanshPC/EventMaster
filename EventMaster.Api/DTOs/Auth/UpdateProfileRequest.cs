namespace EventMaster.Api.DTOs.Auth;
/// <summary>
/// Represents the request to update a user's profile, containing the current password for verification and optional fields for email and phone number that can be updated.
/// </summary>
public class UpdateProfileRequest
{
    public string CurrentPassword { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
