using CateringEcommerce.Domain.Models.Payment;

namespace CateringEcommerce.Domain.Interfaces.Payment
{
    public interface IRazorpayWebhookRepository
    {
        Task<long> CreateWebhookLogAsync(RazorpayWebhookLogEntry entry);
        Task UpdateWebhookLogAsync(long webhookLogId, string processingStatus, string? errorMessage = null);
        Task<bool> IsPaymentSuccessfulAsync(string paymentId);
        Task UpsertPaymentTransactionAsync(RazorpayPaymentTransactionUpsert transaction);
    }
}
