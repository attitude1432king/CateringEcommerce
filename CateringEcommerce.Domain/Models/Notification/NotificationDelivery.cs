namespace CateringEcommerce.Domain.Models.Notification
{
    public class NotificationDelivery
    {
        public long Id { get; set; }
        public string NotificationId { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public string? ProviderMessageId { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public decimal? Cost { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
