using EventMaster.Web.Models;
using EventMaster.Web.Services;
using EventMaster.Web.Services.ApiDtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventMaster.Web.Controllers;

public class AccountController : Controller
{
    private readonly AuthApiClient _authApi;

    public AccountController(AuthApiClient authApi)
    {
        _authApi = authApi;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // if already logged in, go back
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? "/");

        return View(new LoginRegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRegisterViewModel vm)
    {
        var token = await _authApi.LoginAsync(new UserLoginRequest
        {
            UsernameOrEmail = (vm.LoginUsernameOrEmail ?? "").Trim(),
            Password = vm.LoginPassword ?? ""
        });

        if (token is null || string.IsNullOrWhiteSpace(token.Token))
        {
            vm.Error = "Invalid credentials.";
            return View("Login", vm);
        }

        var me = await _authApi.MeAsync(token.Token);
        if (me is null)
        {
            vm.Error = "Login succeeded but user session could not be created.";
            return View("Login", vm);
        }

        await SignInCookieAsync(me, token.Token);

        return LocalRedirect(SafeReturnUrl(vm.ReturnUrl, me.Role));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(LoginRegisterViewModel vm)
    {
        var token = await _authApi.RegisterAsync(new UserRegisterRequest
        {
            Name = (vm.Name ?? "").Trim(),
            Age = vm.Age,
            Phone = vm.Phone.Trim(),
            Email = (vm.Email ?? "").Trim(),
            Username = (vm.Username ?? "").Trim(),
            Password = vm.Password ?? "",
        });

        if (token is null || string.IsNullOrWhiteSpace(token.Token))
        {
            vm.Error = "Registration failed. Please check your inputs (email/username/phone may already exist).";
            return View("Login", vm);
        }

        var me = await _authApi.MeAsync(token.Token);
        if (me is null)
        {
            vm.Error = "Registration succeeded but user session could not be created.";
            return View("Login", vm);
        }

        await SignInCookieAsync(me, token.Token);

        return LocalRedirect(SafeReturnUrl(vm.ReturnUrl, me.Role));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task SignInCookieAsync(MeResponse me, string jwt)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, me.UserId.ToString()),
            new(ClaimTypes.Name, me.Name),
            new(ClaimTypes.Role, me.Role),

            // store JWT for later API calls (optional but useful)
            new("access_token", jwt)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            });
    }

    private string SafeReturnUrl(string? returnUrl, string role)
    {
        // only allow local redirects
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return returnUrl;

        if (string.Equals(role, "CUSTOMER", StringComparison.OrdinalIgnoreCase)) return "/Dashboard/Customer";
        if (string.Equals(role, "ORGANIZER", StringComparison.OrdinalIgnoreCase)) return "/Dashboard/Organizer";
        return "/";
    }
}