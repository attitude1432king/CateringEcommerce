using CateringEcommerce.Domain.Models.Payment;

namespace CateringEcommerce.Domain.Interfaces.Payment
{
    public interface IRazorpayWebhookService
    {
        Task<RazorpayWebhookProcessingResult> ProcessAsync(RazorpayWebhookRequest request);
    }
}
