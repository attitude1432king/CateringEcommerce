namespace CateringEcommerce.Domain.Models.Notification
{
    public class NotificationTemplate
    {
        public long TemplateId { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string Channel { get; set; } = string.Empty; // EMAIL, SMS, INAPP
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
    }

    public class TemplateNotFoundException : Exception
    {
        public string TemplateCode { get; }

        public TemplateNotFoundException(string templateCode)
            : base($"Template '{templateCode}' not found")
        {
            TemplateCode = templateCode;
        }
    }
}
