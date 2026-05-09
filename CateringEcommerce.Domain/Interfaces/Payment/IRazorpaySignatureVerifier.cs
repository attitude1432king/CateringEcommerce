namespace CateringEcommerce.Domain.Interfaces.Payment
{
    public interface IRazorpaySignatureVerifier
    {
        bool VerifyWebhookSignature(string rawBody, string signature);
    }
}
