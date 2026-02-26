namespace EventMaster.Api.Security;
/// <summary>
/// Represents the configuration options for JSON Web Tokens (JWT) used in the authentication and authorization processes of the application.
/// This class contains properties that define the issuer, audience, signing key, and expiration time for JWTs.
/// </summary>
public class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 240;
}
