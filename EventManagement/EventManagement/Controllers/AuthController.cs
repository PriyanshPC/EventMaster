using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketManagement.Api.Controllers
{
    /// <summary> 
    /// This controller handles everything related to user accounts, 
    /// Registering a new user and logging in. 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService) => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var user = await _authService.RegisterAsync(dto);
            if (user == null) return BadRequest("Username or email already exists.");
            return Ok(new { user.UserId, user.Name, user.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _authService.LoginAsync(dto);
            if (user == null) return Unauthorized("Invalid credentials.");
            return Ok(new { user.UserId, user.Name, user.Role });
        }
    }
}
