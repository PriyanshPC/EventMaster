using System.ComponentModel.DataAnnotations;

namespace EventMaster.Web.Models;
/// <summary>
/// ViewModel for the login and registration page, which contains properties for both the login form and the registration form. The ViewModel includes properties for capturing user input for logging in (username/email and password) as well as properties for registering a new account (name, age, phone, email, username, password). It also has a ReturnUrl property to redirect users back to their original destination after successful login or registration, and an Error property to display any error messages that occur during the login or registration process. This ViewModel is used by the LoginRegister view to render both forms and handle user input for authentication and account creation.
/// </summary>
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