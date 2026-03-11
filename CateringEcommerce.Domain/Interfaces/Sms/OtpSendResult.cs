namespace CateringEcommerce.Domain.Interfaces.Sms
{
    public class OtpSendResult
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public string ProviderName { get; set; } = string.Empty;
    }
}
