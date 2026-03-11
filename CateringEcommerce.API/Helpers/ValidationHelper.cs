using System.Text.RegularExpressions;

namespace CateringEcommerce.API.Helpers
{
    /// <summary>
    /// Helper class for email and phone validation using regex
    /// </summary>
    public static class ValidationHelper
    {
        // Email regex pattern - RFC 5322 simplified
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Phone regex pattern - allows 10 digits with optional country code, spaces, hyphens, parentheses
        // Examples: 9876543210, +91-9876543210, (98) 7654-3210
        private static readonly Regex PhoneRegex = new Regex(
            @"^(?:\+\d{1,3}[-.\s]?)?\(?(\d{3})\)?[-.\s]?(\d{3})[-.\s]?(\d{4})$",
            RegexOptions.Compiled);

        /// <summary>
        /// Validates email address using regex pattern
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (email.Length > 254)
                return false;

            return EmailRegex.IsMatch(email);
        }

        /// <summary>
        /// Validates phone number using regex pattern
        /// Accepts formats like: 9876543210, +91-9876543210, (98) 7654-3210, etc.
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove all non-digit characters except leading +
            string cleanedPhone = Regex.Replace(phone, @"[^\d+]", "");

            // Check if it's a valid format (10 digits minimum for local, 11+ for international)
            if (cleanedPhone.StartsWith("+"))
            {
                return cleanedPhone.Length >= 12 && cleanedPhone.Length <= 15;
            }

            return cleanedPhone.Length == 10 && Regex.IsMatch(cleanedPhone, @"^\d{10}$");
        }

        /// <summary>
        /// Extracts and normalizes phone number (removes formatting characters)
        /// </summary>
        public static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            return Regex.Replace(phone, @"[^\d+]", "");
        }

        /// <summary>
        /// Normalizes email to lowercase
        /// </summary>
        public static string NormalizeEmail(string email)
        {
            return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLower();
        }
    }
}
