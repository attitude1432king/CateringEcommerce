namespace CateringEcommerce.Domain.Models.Notification
{
    public class AwsSnsSettings
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = "ap-south-1";
        public string SenderId { get; set; } = "ENYVORA";
    }
}
