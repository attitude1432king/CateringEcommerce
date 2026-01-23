namespace CateringEcommerce.Domain.Models.Notification
{
    public class InAppNotification
    {
        public string NotificationId { get; set; }
        public string UserId { get; set; }
        public NotificationAudience UserType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
        public string ActionUrl { get; set; }
        public string IconUrl { get; set; }
        public object Data { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}