using Org.BouncyCastle.Crypto.Generators;
using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.BAL.Helpers
{
    public static class HashHelper
    {
        /// <summary>
        /// Hash password using BCrypt (secure hashing with salt)
        /// </summary>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verify password against stored hash - supports both BCrypt (new) and SHA256 (legacy)
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                // Try BCrypt verification first (new secure method)
                if (storedHash.StartsWith("$2") || storedHash.StartsWith("$2a") || storedHash.StartsWith("$2b") || storedHash.StartsWith("$2y"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }

                // Fall back to SHA256 for legacy support (will be phased out)
                // This allows gradual migration from SHA256 to BCrypt
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var sha256Hash = Convert.ToBase64String(hashedBytes);
                    return sha256Hash == storedHash;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Password verification failed: {ex.Message}");
                return false;
            }
        }
    }

}
