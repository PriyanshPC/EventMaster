using EventMaster.Api.Data;
using EventMaster.Api.DTOs.Auth;
using EventMaster.Api.Entities;
using EventMaster.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventMaster.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly EventMasterDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenService _jwt;
    private readonly CurrentUser _me;

    public AuthController(EventMasterDbContext db, PasswordHasher hasher, JwtTokenService jwt, CurrentUser me)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _me = me;
    }

    // =========================
    // Register (CUSTOMER only)
    // =========================
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        req.Name = (req.Name ?? "").Trim();
        req.Email = (req.Email ?? "").Trim();
        req.Username = (req.Username ?? "").Trim();
        req.Phone = req.Phone?.Trim();

        if (!AuthValidation.IsValidFullName(req.Name))
            return BadRequest("Name must be 'First Last' with exactly one space (no middle name, no extra spaces).");

        if (req.Age < 18)
            return BadRequest("Age must be at least 18.");

        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest("Email is required.");

        if (!AuthValidation.IsValidEmail(req.Email))
            return BadRequest("Email format is invalid.");

        if (string.IsNullOrWhiteSpace(req.Username))
            return BadRequest("Username is required.");

        if (!string.IsNullOrWhiteSpace(req.Phone) && !AuthValidation.IsValidPhone(req.Phone))
            return BadRequest("Phone number must be 10 digits and start with 2-9.");

        if (!AuthValidation.IsStrongPassword(req.Password))
            return BadRequest("Password must be at least 8 characters and include 1 uppercase, 1 number, and 1 special character.");

        var emailExists = await _db.users.AnyAsync(u => u.email == req.Email);
        if (emailExists) return Conflict("Email already exists.");

        var usernameExists = await _db.users.AnyAsync(u => u.username == req.Username);
        if (usernameExists) return Conflict("Username already exists.");

        var phoneExists = await _db.users.AnyAsync(u => u.phone == req.Phone);
        if (phoneExists) return Conflict("Phone already exists.");


        var user = new user
        {
            role = "CUSTOMER",
            name = req.Name,
            age = req.Age,
            phone = req.Phone,
            email = req.Email,
            username = req.Username,
            password = _hasher.Hash(req.Password),
            status = "Active"
        };

        _db.users.Add(user);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Duplicate value detected.");
        }

        return Ok(new AuthResponse { Token = _jwt.CreateToken(user) });
    }


    // =========================
    // Login
    // =========================
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var key = (req.UsernameOrEmail ?? "").Trim();
        var u = await _db.users.FirstOrDefaultAsync(x => x.username == key || x.email == key);

        if (u is null) return Unauthorized("Invalid credentials.");

        // block deactivated users
        if (!string.Equals(u.status, "Active", StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Account is deactivated.");

        if (!_hasher.Verify(req.Password, u.password))
            return Unauthorized("Invalid credentials.");

        return Ok(new AuthResponse { Token = _jwt.CreateToken(u) });
    }

    // =========================
    // Me
    // =========================
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var u = await _db.users.FirstOrDefaultAsync(x => x.user_id == _me.UserId);
        if (u is null) return Unauthorized();

        if (!string.Equals(u.status, "Active", StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Account is deactivated.");

        return Ok(new MeResponse
        {
            UserId = u.user_id,
            Role = u.role,
            Username = u.username,
            Email = u.email,
            Name = u.name,
            Phone = u.phone
        });
    }

    // =========================
    // Update email OR phone (one at a time) + current password + 24h cooldown
    // =========================
    [HttpPatch("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var u = await _db.users.FirstOrDefaultAsync(x => x.user_id == _me.UserId);
        if (u is null) return Unauthorized();

        if (!string.Equals(u.status, "Active", StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Account is deactivated.");

        if (string.IsNullOrWhiteSpace(req.CurrentPassword))
            return BadRequest("Current password is required.");

        if (!_hasher.Verify(req.CurrentPassword, u.password))
            return Unauthorized("Current password is incorrect.");

        // 24h cooldown for ANY profile updates (you approved this)
        if (DateTime.UtcNow - u.updated_at < TimeSpan.FromHours(24))
            return Conflict("Profile updates are limited to once every 24 hours.");

        var newEmail = req.Email?.Trim();
        var newPhone = req.Phone?.Trim();

        var wantsEmail = !string.IsNullOrWhiteSpace(newEmail);
        var wantsPhone = !string.IsNullOrWhiteSpace(newPhone);

        if (wantsEmail == wantsPhone)
            return BadRequest("Provide exactly one field: email OR phone.");

        if (wantsEmail)
        {
            if (!newEmail!.Contains('@') || !newEmail.Contains('.'))
                return BadRequest("Email format is invalid.");

            var exists = await _db.users.AnyAsync(x => x.user_id != u.user_id && x.email == newEmail);
            if (exists) return Conflict("Email already exists.");

            u.email = newEmail;
        }
        else
        {
            if (!AuthValidation.IsValidPhone(newPhone))
                return BadRequest("Phone number must be 10 digits and start with 2-9.");
            var phoneExists = await _db.users.AnyAsync(u => u.phone == req.Phone);
            if (phoneExists) return Conflict("Phone already exists.");

            u.phone = newPhone;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Duplicate value detected.");
        }
        return NoContent();
    }


    // =========================
    // Change password + current password + 24h cooldown
    // =========================
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var u = await _db.users.FirstOrDefaultAsync(x => x.user_id == _me.UserId);
        if (u is null) return Unauthorized();

        if (!string.Equals(u.status, "Active", StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Account is deactivated.");

        if (string.IsNullOrWhiteSpace(req.CurrentPassword))
            return BadRequest("Current password is required.");

        if (!_hasher.Verify(req.CurrentPassword, u.password))
            return Unauthorized("Current password is incorrect.");

        if (!AuthValidation.IsStrongPassword(req.NewPassword))
            return BadRequest("Password must be at least 8 characters and include 1 uppercase, 1 number, and 1 special character.");

        // same 24h cooldown rule you approved
        if (DateTime.UtcNow - u.updated_at < TimeSpan.FromHours(24))
            return Conflict("Profile updates are limited to once every 24 hours.");

        u.password = _hasher.Hash(req.NewPassword);
        await _db.SaveChangesAsync();

        return NoContent();
    }


    // =========================
    // Soft delete account
    // =========================
    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeactivateAccount()
    {
        var u = await _db.users.FirstOrDefaultAsync(x => x.user_id == _me.UserId);
        if (u is null)
            return Ok(new { message = "User Deleted" }); // idempotent

        u.status = "Deactivated";
        await _db.SaveChangesAsync();

        return Ok(new { message = "User Deleted" });
    }
}
