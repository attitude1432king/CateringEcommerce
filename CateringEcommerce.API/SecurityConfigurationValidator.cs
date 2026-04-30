using CateringEcommerce.Domain.Models.Configuration;

namespace CateringEcommerce.API
{
    public static class SecurityConfigurationValidator
    {
        private static readonly string[] ProductionRequiredKeys =
        {
            "ConnectionStrings:DefaultConnection",
            "JWT:KEY",
            //"PAYMENT:RAZORPAY_KEY_ID",
            //"PAYMENT:RAZORPAY_KEY_SECRET",
            //"PAYMENT:RAZORPAY_WEBHOOK_SECRET",
            //"MSG91:AUTH_KEY",
            //"EMAIL:SMTP_USERNAME",
            //"EMAIL:SMTP_PASSWORD",
            "SYSTEM:ENCRYPTION_KEY"
        };

        private static readonly string[] PlaceholderMarkers =
        {
            "REPLACE_WITH",
            "change_this",
            "xxxxxxxx",
            "YOUR_",
            "Pankaj@Lohar"
        };

        public static void Validate(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var jwtKey = configuration["JWT:KEY"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT:KEY must be configured through secure configuration or the JWT__KEY environment variable.");
            }

            if (!environment.IsProduction())
            {
                return;
            }

            var missing = ProductionRequiredKeys
                .Where(key => IsMissingOrPlaceholder(configuration[key]))
                .ToArray();

            if (missing.Length > 0)
            {
                throw new InvalidOperationException(
                    "Production secrets are not configured securely. Missing or placeholder values: " +
                    string.Join(", ", missing));
            }
        }

        private static bool IsMissingOrPlaceholder(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return PlaceholderMarkers.Any(marker =>
                value.Contains(marker, StringComparison.OrdinalIgnoreCase));
        }
    }
}
