using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CateringEcommerce.BAL.Notification
{
    public class InAppConsumer : NotificationConsumerBase
    {
        public InAppConsumer(
            IConnection connection,
            ILogger<InAppConsumer> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(connection, logger, serviceScopeFactory)
        {
        }

        protected override string QueueName => "inapp.queue";

        protected override async Task ProcessMessageAsync(
            NotificationMessage message,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();
            var inAppService = scope.ServiceProvider.GetRequiredService<IInAppNotificationService>();

            // Render in-app message
            var renderedMessage = await templateService.RenderTemplateAsync(
                message.TemplateCode,
                message.Recipient.PreferredLanguage ?? "en",
                message.Data,
                cancellationToken);

            // Send in-app notification
            await inAppService.SendInAppNotificationAsync(message, renderedMessage, cancellationToken);
        }
    }
}
