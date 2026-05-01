using Microsoft.Extensions.Configuration;

namespace CateringEcommerce.Domain.Models.Configuration
{
    public class JwtSettings
    {
        public const string SectionName = "JWT";

        [ConfigurationKeyName("KEY")]
        public string Key { get; set; } = string.Empty;

        [ConfigurationKeyName("ISSUER")]
        public string Issuer { get; set; } = string.Empty;

        [ConfigurationKeyName("AUDIENCE")]
        public string Audience { get; set; } = string.Empty;

        [ConfigurationKeyName("EXPIRE_MINUTES")]
        public int ExpireMinutes { get; set; } = 1440;
    }

    public class Msg91Settings
    {
        public const string SectionName = "MSG91";

        [ConfigurationKeyName("AUTH_KEY")]
        public string AuthKey { get; set; } = string.Empty;

        [ConfigurationKeyName("SENDER_ID")]
        public string SenderId { get; set; } = "CATAPP";

        [ConfigurationKeyName("TEMPLATE_ID")]
        public string TemplateId { get; set; } = string.Empty;

        [ConfigurationKeyName("ROUTE")]
        public string Route { get; set; } = "4";
    }

    public class RazorpaySettings
    {
        public const string SectionName = "PAYMENT";

        [ConfigurationKeyName("RAZORPAY_KEY_ID")]
        public string KeyId { get; set; } = string.Empty;

        [ConfigurationKeyName("RAZORPAY_KEY_SECRET")]
        public string KeySecret { get; set; } = string.Empty;

        [ConfigurationKeyName("RAZORPAY_WEBHOOK_SECRET")]
        public string WebhookSecret { get; set; } = string.Empty;
    }

    public class SmtpSettings
    {
        public const string SectionName = "EMAIL";

        [ConfigurationKeyName("SMTP_HOST")]
        public string Host { get; set; } = "smtp.gmail.com";

        [ConfigurationKeyName("SMTP_PORT")]
        public int Port { get; set; } = 587;

        [ConfigurationKeyName("SMTP_USERNAME")]
        public string Username { get; set; } = string.Empty;

        [ConfigurationKeyName("SMTP_PASSWORD")]
        public string Password { get; set; } = string.Empty;

        [ConfigurationKeyName("ENABLE_SSL")]
        public bool EnableSsl { get; set; } = true;

        [ConfigurationKeyName("FROM_ADDRESS")]
        public string FromAddress { get; set; } = string.Empty;

        [ConfigurationKeyName("FROM_NAME")]
        public string FromName { get; set; } = "Enyvora Catering";
    }

    public class SmsGatewaySettings
    {
        public const string SectionName = "SMS";

        [ConfigurationKeyName("API_KEY")]
        public string ApiKey { get; set; } = string.Empty;

        [ConfigurationKeyName("SENDER_ID")]
        public string SenderId { get; set; } = "ENYVORA";
    }

    public class SecuritySettings
    {
        public const string SectionName = "SYSTEM";

        [ConfigurationKeyName("ENCRYPTION_KEY")]
        public string EncryptionKey { get; set; } = string.Empty;
    }
}
