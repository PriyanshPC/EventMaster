using EventTicketManagement.Api.Data;
using EventTicketManagement.Api.DTOs;
using EventTicketManagement.Api.Helpers;
using EventTicketManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventTicketManagement.Api.Services
{
    /// <summary> 
    ///  This service handles user registration and login. /// It talks to the database and checks if users exist, 
    ///  and also verifies passwords. 
    ///  </summary>
    public class AuthService
    {
        private readonly AppDbContext _context;
        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> RegisterAsync(UserRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                return null;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Username = dto.Username,
                Role = dto.Role,
                Password = PasswordHelper.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> LoginAsync(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return null;
            if (!PasswordHelper.VerifyPassword(dto.Password, user.Password)) return null;
            return user;
        }
    }
}
