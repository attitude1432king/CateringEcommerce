namespace CateringECommerce.BAL.Configuration
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        public static readonly string AuthToken = "32d72da8576aab7007b38891731caf34";
        public static readonly string FromPhoneNumber = "81601 82327";
    }
}
