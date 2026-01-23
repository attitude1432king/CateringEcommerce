using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CateringEcommerce.BAL.Notification
{
    public class EmailConsumer : NotificationConsumerBase
    {
        public EmailConsumer(
            IConnection connection,
            ILogger<EmailConsumer> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(connection, logger, serviceScopeFactory)
        {
        }

        protected override string QueueName => "email.queue";

        protected override async Task ProcessMessageAsync(
            NotificationMessage message,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Render subject
            var subject = await templateService.RenderSubjectAsync(
                message.TemplateCode,
                message.Recipient.PreferredLanguage ?? "en",
                message.Data,
                cancellationToken);

            // Add subject to data for email body
            message.Data["subject"] = subject;

            // Render email body
            var renderedHtml = await templateService.RenderTemplateAsync(
                message.TemplateCode,
                message.Recipient.PreferredLanguage ?? "en",
                message.Data,
                cancellationToken);

            // Send email
            await emailService.SendEmailAsync(message, renderedHtml, cancellationToken);
        }
    }
}
