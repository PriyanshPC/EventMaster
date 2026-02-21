using System.ComponentModel.DataAnnotations;

namespace EventMaster.Web.Models;

public class LoginRegisterViewModel
{
    public string? ReturnUrl { get; set; }

    // Login form
    [Display(Name = "Username or Email")]
    public string LoginUsernameOrEmail { get; set; } = "";

    [Display(Name = "Password")]
    public string LoginPassword { get; set; } = "";

    // Register form
    public string Name { get; set; } = "";
    public int Age { get; set; } = 18;
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public string? Error { get; set; }
}