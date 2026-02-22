namespace EventMaster.Web.Services.ApiDtos;

public class UserLoginRequest
{
    public string UsernameOrEmail { get; set; } = "";
    public string Password { get; set; } = "";
}

public class UserRegisterRequest
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class AuthResponse
{
    public string Token { get; set; } = "";
}

public class MeResponse
{
    public int UserId { get; set; }
    public string Role { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
}

public class UpdateProfileRequest
{
    public string? Email { get; set; } = "";
    public string? Phone { get; set; } = "";
    public string CurrentPassword { get; set; } = "";
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
public class ApiOperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
