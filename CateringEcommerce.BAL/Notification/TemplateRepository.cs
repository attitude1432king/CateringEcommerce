using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.BAL.Notification
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public TemplateRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Stub methods - To be implemented
        public async Task<NotificationTemplate?> GetActiveTemplateAsync(
            string templateCode,
            string language,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task<NotificationTemplate?> GetTemplateByIdAsync(long templateId)
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task<List<NotificationTemplate>> GetAllTemplatesAsync(
            string? channel = null,
            string? category = null)
        {
            await Task.CompletedTask;
            return new List<NotificationTemplate>();
        }

        public async Task<long> CreateTemplateAsync(NotificationTemplate template)
        {
            await Task.CompletedTask;
            return 0;
        }

        public async Task UpdateTemplateAsync(NotificationTemplate template)
        {
            await Task.CompletedTask;
        }

        public async Task DeleteTemplateAsync(long templateId)
        {
            await Task.CompletedTask;
        }

        public async Task IncrementUsageCountAsync(long templateId)
        {
            await Task.CompletedTask;
        }
    }
}
