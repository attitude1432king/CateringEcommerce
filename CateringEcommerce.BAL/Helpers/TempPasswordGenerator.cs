using System.Security.Cryptography;

namespace CateringEcommerce.BAL.Helpers
{
    public static class TempPasswordGenerator
    {
        private const string Upper   = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lower   = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits  = "0123456789";
        private const string Special = "!@#$%^&*()_+-=";
        private const string All     = Upper + Lower + Digits + Special;

        /// <summary>
        /// Generates a cryptographically secure temporary password of at least 12 characters.
        /// Guarantees at least one uppercase, one lowercase, one digit, and one special character.
        /// </summary>
        public static string Generate(int length = 12)
        {
            if (length < 10)
                throw new ArgumentException("Temporary password length must be at least 10.", nameof(length));

            var result = new char[length];

            // Guarantee at least one character from each required class
            result[0] = Pick(Upper);
            result[1] = Pick(Lower);
            result[2] = Pick(Digits);
            result[3] = Pick(Special);

            for (int i = 4; i < length; i++)
                result[i] = Pick(All);

            // Fisher-Yates shuffle using CSPRNG to randomise position of required chars
            for (int i = length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return new string(result);
        }

        private static char Pick(string chars) =>
            chars[RandomNumberGenerator.GetInt32(chars.Length)];
    }
}
