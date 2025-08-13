using CateringEcommerce.Domain.Interfaces;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace CateringEcommerce.BAL.Configuration
{ 
     public class SmsService: ISmsService
    {
         
        private readonly string AccountSid;
        private readonly string AuthToken;
        private readonly string VerifyServiceSID;


        public SmsService(IConfiguration config)
        {
            AccountSid = ""; // "ACa0e64157a3eeacc75d11d3ca0a45dc58";
            AuthToken = ""; //"8a6511feae7fab86efcd8d70c10a96f3";
            VerifyServiceSID = ""; // "VA36c73036f1c116da2e88220cdcf48834";

            TwilioClient.Init(AccountSid, AuthToken);
        }

        public void SendOtp(string phoneNumber)
        {
            VerificationResource.Create(
                to: phoneNumber,
                channel: "sms",
                pathServiceSid: VerifyServiceSID
            );
        }

        public bool VerifyOtp(string phoneNumber, string code)
        {
            var result = VerificationCheckResource.Create(
                to: phoneNumber,
                code: code,
                pathServiceSid: VerifyServiceSID
            );

            return result.Status == "approved";
        }
    }
}
