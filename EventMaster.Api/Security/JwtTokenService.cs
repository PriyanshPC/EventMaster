using EventMaster.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventMaster.Api.Security;

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
