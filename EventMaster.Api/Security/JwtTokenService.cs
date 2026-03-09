using EventMaster.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventMaster.Api.Security;
/// <summary>
/// Provides functionality to create JSON Web Tokens (JWT) for authenticated users in the application. This service takes user information and generates a JWT that can be used for authentication and authorization purposes in subsequent requests.
/// The CreateToken method accepts a user object (which should contain properties such as user ID, username, role, email, and display name) and constructs a JWT with claims based on this information. The token is signed using a symmetric security key derived from the configuration options provided through the JwtOptions class.
/// The generated token includes claims for the user's ID, username, role, email, and display name, and it is set to expire after a specified duration defined in the JwtOptions. This service is typically used in the authentication workflow of the application, where after validating user credentials, a JWT is created and returned to the client for use in authenticating future requests.
/// </summary>
public class JwtTokenService
{
    private readonly JwtOptions _opts;

    public JwtTokenService(IOptions<JwtOptions> opts)
    {
        _opts = opts.Value;
    }

    public string CreateToken(user u) // if your class name is 'users', change user -> users
    {
        var role = u.role ?? "CUSTOMER";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.user_id.ToString()),
            new(ClaimTypes.NameIdentifier, u.user_id.ToString()),
            new(ClaimTypes.Name, u.username ?? ""),
            new(ClaimTypes.Role, role),
            new("email", u.email ?? ""),
            new("displayName", u.name ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opts.ExpiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
