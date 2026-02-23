using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace EventTicketManagement.Api.Helpers
{
    /// <summary>
    /// Helper class for password hashing and verification. Provides methods to hash a plain text password using a secure algorithm (PBKDF2) and to verify a plain text password against a stored hash. The HashPassword method generates a random salt and hashes the password, returning a string that combines the salt and the hash. The VerifyPassword method splits the stored hash to retrieve the salt and compares the hash of the provided password with the stored hash to determine if they match. This class is used to securely handle user passwords in the application, ensuring that passwords are not stored in plain text and that they can be verified during authentication processes.
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a plain text password using PBKDF2 with a randomly generated salt. The method generates a 16-byte salt, hashes the password with the salt using HMACSHA256, and returns a string that combines the salt and the hash in a format that can be stored in the database. The resulting string is in the format "salt:hash", where both the salt and hash are Base64-encoded. This method ensures that passwords are securely hashed and that the salt is unique for each password, providing protection against rainbow table attacks.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password, salt, KeyDerivationPrf.HMACSHA256, 10000, 32));
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

/// <summary>
/// Verifies a plain text password against a stored hash. The method splits the stored hash to retrieve the salt and the hash, then hashes the provided password with the retrieved salt using the same algorithm and parameters as the HashPassword method. Finally, it compares the computed hash with the stored hash and returns true if they match, indicating that the provided password is correct, or false otherwise. This method is essential for authenticating users by verifying their passwords during login attempts.
/// </summary>
/// <param name="password"></param>
/// <param name="storedHash"></param>
/// <returns></returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password, salt, KeyDerivationPrf.HMACSHA256, 10000, 32));

            return hash == parts[1];
        }
    }
}
