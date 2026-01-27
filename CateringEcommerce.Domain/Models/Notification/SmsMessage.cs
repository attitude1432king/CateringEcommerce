namespace CateringEcommerce.Domain.Models.Notification
{
    public class SmsMessage
    {
        public string To { get; set; }              // E.164 format: +919876543210
        public string Message { get; set; }
        public string From { get; set; }            // Sender ID
        public bool IsOtp { get; set; }             // OTP messages have special routing
        public int ValidityPeriod { get; set; }     // In minutes
    }
}
