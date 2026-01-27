using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CateringEcommerce.BAL.Notification
{
    public class SmsConsumer : NotificationConsumerBase
    {
        public SmsConsumer(
            IConnection connection,
            ILogger<SmsConsumer> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(connection, logger, serviceScopeFactory)
        {
        }

        protected override string QueueName => "sms.queue";

        protected override async Task ProcessMessageAsync(
            NotificationMessage message,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();
            var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();

            // Render SMS body
            var renderedMessage = await templateService.RenderTemplateAsync(
                message.TemplateCode,
                message.Recipient.PreferredLanguage ?? "en",
                message.Data,
                cancellationToken);

            // Validate character limit
            if (renderedMessage.Length > 160 && !message.Category.Contains("OTP"))
            {
                _logger.LogWarning(
                    "SMS message exceeds 160 characters ({Length}). MessageId: {MessageId}",
                    renderedMessage.Length, message.MessageId);

                // Truncate to 160 characters
                renderedMessage = renderedMessage.Substring(0, 157) + "...";
            }

            // Send SMS
            await smsService.SendSmsAsync(message, renderedMessage, cancellationToken);
        }
    }
}
