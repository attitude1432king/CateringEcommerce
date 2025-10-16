namespace CateringEcommerce.Domain.Interfaces
{
    public interface ISmsService
    {
        void SendOtp(string phoneNumber);
        bool VerifyOtp(string phoneNumber, string code);
    }
}
