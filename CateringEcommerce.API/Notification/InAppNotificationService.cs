using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace CateringEcommerce.API.Notification
{
    public class InAppNotificationService : IInAppNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<InAppNotificationService> _logger;
        private readonly INotificationRepository _repository;

        public InAppNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<InAppNotificationService> logger,
            INotificationRepository repository)
        {
            _hubContext = hubContext;
            _logger = logger;
            _repository = repository;
        }

        public async Task SendInAppNotificationAsync(
            NotificationMessage notification,
            string renderedMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                // Store in database first
                var inAppNotification = new InAppNotification
                {
                    NotificationId = notification.MessageId,
                    UserId = notification.Recipient.Id,
                    UserType = notification.Audience,
                    Title = notification.Data.ContainsKey("title")
                        ? notification.Data["title"]?.ToString()
                        : "Notification",
                    Message = renderedMessage,
                    Category = notification.Category,
                    Priority = (int)notification.Priority,
                    ActionUrl = notification.Data.ContainsKey("actionUrl")
                        ? notification.Data["actionUrl"]?.ToString()
                        : null,
                    IconUrl = notification.Data.ContainsKey("iconUrl")
                        ? notification.Data["iconUrl"]?.ToString()
                        : null,
                    Data = JsonSerializer.Serialize(notification.Data),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = notification.Options?.ExpiresAt
                };

                await _repository.SaveInAppNotificationAsync(inAppNotification);

                // Send via SignalR
                var dto = new InAppNotificationDto
                {
                    NotificationId = inAppNotification.NotificationId,
                    Title = inAppNotification.Title,
                    Message = inAppNotification.Message,
                    Category = inAppNotification.Category,
                    Priority = inAppNotification.Priority,
                    ActionUrl = inAppNotification.ActionUrl,
                    IconUrl = inAppNotification.IconUrl,
                    IsRead = false,
                    CreatedAt = inAppNotification.CreatedAt
                };

                // Send to specific user
                var groupName = $"{notification.Audience}_{notification.Recipient.Id}";
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", dto, cancellationToken);

                _logger.LogInformation(
                    "In-app notification sent to user {UserId} ({UserType})",
                    notification.Recipient.Id, notification.Audience);

                // Update delivery status
                await _repository.SaveDeliveryStatusAsync(new NotificationDelivery
                {
                    NotificationId = notification.MessageId,
                    Channel = "INAPP",
                    Status = "DELIVERED",
                    Provider = "SignalR",
                    SentAt = DateTime.UtcNow,
                    Recipient = notification.Recipient.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send in-app notification");

                await _repository.SaveDeliveryStatusAsync(new NotificationDelivery
                {
                    NotificationId = notification.MessageId,
                    Channel = "INAPP",
                    Status = "FAILED",
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow,
                    Recipient = notification.Recipient.Id
                });
            }
        }

        // Broadcast to all users of a specific type
        public async Task BroadcastAsync(
            string userType,
            string title,
            string message,
            CancellationToken cancellationToken)
        {
            var dto = new InAppNotificationDto
            {
                NotificationId = Guid.NewGuid().ToString(),
                Title = title,
                Message = message,
                Category = "BROADCAST",
                Priority = (int)NotificationPriority.Low,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(userType).SendAsync("ReceiveNotification", dto, cancellationToken);

            _logger.LogInformation("Broadcast sent to {UserType}", userType);
        }
    }
}
