using CateringEcommerce.Domain.Models.Notification;

namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface IEmailService
    {
        Task<EmailResult> SendEmailAsync(
            NotificationMessage notification,
            string renderedHtml,
            CancellationToken cancellationToken = default);
    }
}
