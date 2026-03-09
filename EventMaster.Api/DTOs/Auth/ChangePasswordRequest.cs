namespace EventMaster.Api.DTOs.Auth;
/// <summary>
/// Represents the request to change a user's password, containing the current password and the new password.
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
