using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.BAL.Helpers
{
    public static class HashHelper
    {
        // Hash a plain text using SHA256
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Convert to hex string
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2")); // hex format

                return sb.ToString();
            }
        }

        // Compare raw password with hashed password
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            string hashOfInput = HashPassword(inputPassword);
            return hashOfInput.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }

}
