using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace CateringEcommerce.BAL.Common
{
    public static class Utils
    {
        /// <summary>
        /// Gete a random OTP (One Time Password) of specified length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateOtp(int length = 6)
        {
            var random = new Random();
            var otp = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                otp.Append(random.Next(0, 10));
            }

            return otp.ToString();
        }

        public static string GetEnumDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                                  .FirstOrDefault() as DisplayAttribute;

            return attribute?.Name ?? value.ToString();
        }

        /// <summary>
        /// Generate a cryptographically secure random temporary password
        /// Includes uppercase, lowercase, digits, and special characters
        /// </summary>
        /// <param name="length">Password length (minimum 12 characters recommended)</param>
        /// <returns>Secure random password</returns>
        public static string GenerateSecureTemporaryPassword(int length = 16)
        {
            if (length < 12)
                throw new ArgumentException("Password length must be at least 12 characters for security");

            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // Exclude I, O for readability
            const string lowercase = "abcdefghijkmnpqrstuvwxyz"; // Exclude l, o for readability
            const string digits = "23456789"; // Exclude 0, 1 for readability
            const string special = "!@#$%&*";

            var password = new StringBuilder();

            // Ensure at least one character from each category
            password.Append(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
            password.Append(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
            password.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
            password.Append(special[RandomNumberGenerator.GetInt32(special.Length)]);

            // Fill the rest randomly
            string allChars = uppercase + lowercase + digits + special;
            for (int i = password.Length; i < length; i++)
            {
                password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
            }

            // Shuffle the password to randomize the position of guaranteed characters
            return new string(password.ToString().OrderBy(x => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }

        /// <summary>
        /// Generate a secure PIN for numeric-only passwords
        /// </summary>
        /// <param name="length">PIN length (default 6)</param>
        /// <returns>Cryptographically secure numeric PIN</returns>
        public static string GenerateSecurePin(int length = 6)
        {
            var pin = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                pin.Append(RandomNumberGenerator.GetInt32(0, 10));
            }
            return pin.ToString();
        }

    }
}
