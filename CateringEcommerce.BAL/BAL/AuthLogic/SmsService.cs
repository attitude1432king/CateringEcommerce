using CateringECommerce.BAL.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CateringEcommerce.BAL.BAL.AuthLogic
{ 
     public class SmsService
     {
        public SmsService()
        {
            TwilioClient.Init(TwilioSettings.AccountSid, TwilioSettings.AuthToken);
        }

        public async Task SendOtpAsync(string toPhoneNumber, string otp)
        {
            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(TwilioSettings.FromPhoneNumber),
                body: $"Your OTP is {otp}");

            Console.WriteLine($"OTP sent to {toPhoneNumber}: {message.Sid}");
        }
    }
}
