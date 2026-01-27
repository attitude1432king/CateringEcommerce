namespace CateringEcommerce.Domain.Models.Notification
{
    public class EmailResult
    {
        public bool Success { get; set; }
        public string MessageId { get; set; }
        public string ProviderMessageId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
        public string ProviderName { get; set; }
    }
}
