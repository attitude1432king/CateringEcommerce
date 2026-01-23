namespace CateringEcommerce.Domain.Models.Notification
{
    public class InAppNotificationDto
    {
        public string NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
        public string ActionUrl { get; set; }
        public string IconUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
