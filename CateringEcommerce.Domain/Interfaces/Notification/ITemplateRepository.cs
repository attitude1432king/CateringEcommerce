using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface ITemplateRepository
    {
        Task<NotificationTemplate?> GetActiveTemplateAsync(
            string templateCode,
            string language,
            CancellationToken cancellationToken = default);

        Task<NotificationTemplate?> GetTemplateByIdAsync(long templateId);

        Task<List<NotificationTemplate>> GetAllTemplatesAsync(
            string? channel = null,
            string? category = null);

        Task<long> CreateTemplateAsync(NotificationTemplate template);

        Task UpdateTemplateAsync(NotificationTemplate template);

        Task DeleteTemplateAsync(long templateId);

        Task IncrementUsageCountAsync(long templateId);
    }
}
