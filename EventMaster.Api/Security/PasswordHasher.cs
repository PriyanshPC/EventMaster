namespace EventMaster.Api.Security;
/// <summary>
/// Provides functionality to hash and verify passwords using the BCrypt algorithm. This class offers two primary methods: Hash, which takes a plaintext password and returns a securely hashed version of it, and Verify, which checks if a given plaintext password matches a previously hashed password.
/// The Hash method uses the BCrypt algorithm to generate a salted hash of the password, which helps protect against rainbow table attacks and ensures that even if two users have the same password, their hashes will be different due to the unique salt. The Verify method compares a plaintext password with a hashed password and returns true if they match, allowing for secure authentication processes in the application.
/// This class is typically used in the authentication workflow of the application, where user passwords need to be securely stored in the database and later verified during login attempts without exposing the plaintext passwords. By using BCrypt, this implementation provides a strong level of security for password management in the application.
/// </summary>
public class PasswordHasher
{
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
