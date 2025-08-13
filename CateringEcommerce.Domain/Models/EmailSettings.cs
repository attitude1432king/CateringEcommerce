namespace CateringEcommerce.Domain.Models
{
    public class EmailSettings
    {
        public string FromEmail { get; set; }
        public string AppPassword { get; set; }
    }

    public class OtpEntry
    {
        public string Otp { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

}
