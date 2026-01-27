using CateringEcommerce.Domain.Models.Notification;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CateringEcommerce.API.Notification
{
    public abstract class NotificationConsumerBase : BackgroundService
    {
        protected readonly IConnection _connection;
        protected readonly IModel _channel;
        protected readonly ILogger _logger;
        protected readonly IServiceScopeFactory _serviceScopeFactory;

        protected NotificationConsumerBase(
            IConnection connection,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected abstract string QueueName { get; }
        protected abstract Task ProcessMessageAsync(NotificationMessage message, CancellationToken cancellationToken);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            // Set prefetch count for fair dispatch
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation(
                        "Received message from {Queue}. DeliveryTag: {DeliveryTag}",
                        QueueName, ea.DeliveryTag);

                    var notification = JsonSerializer.Deserialize<NotificationMessage>(message);

                    await ProcessMessageAsync(notification, stoppingToken);

                    // Acknowledge the message
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                    _logger.LogInformation(
                        "Message processed successfully. MessageId: {MessageId}",
                        notification.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");

                    // Reject and requeue if retry count < max
                    var requeue = (ea.BasicProperties?.Headers != null &&
                        ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryCountObj) &&
                        Convert.ToInt32(retryCountObj) < 3);

                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: requeue);

                    if (!requeue)
                    {
                        _logger.LogWarning(
                            "Message moved to DLQ after max retries. DeliveryTag: {DeliveryTag}",
                            ea.DeliveryTag);
                    }
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("{ConsumerName} started consuming from {Queue}",
                GetType().Name, QueueName);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
