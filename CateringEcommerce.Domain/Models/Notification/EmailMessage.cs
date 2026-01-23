namespace CateringEcommerce.Domain.Models.Notification
{
    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ReplyTo { get; set; }
        public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public bool TrackOpens { get; set; }
        public bool TrackClicks { get; set; }
    }
}
