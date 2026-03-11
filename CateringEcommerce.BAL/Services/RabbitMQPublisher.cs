using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Services
{
    /// <summary>
    /// RabbitMQ Publisher for notification messages
    /// </summary>
    public class RabbitMQPublisher : IDisposable
    {
        private readonly ILogger<RabbitMQPublisher> _logger;
        private readonly RabbitMQSettings _settings;
        private bool _isEnabled;
        private IConnection? _connection;
        private IModel? _channel;
        private bool _disposed = false;

        public RabbitMQPublisher(ILogger<RabbitMQPublisher> logger, RabbitMQSettings settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _isEnabled = settings.Enabled;

            if (_isEnabled)
            {
                try
                {
                    InitializeConnection();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize RabbitMQ connection. Running in fallback mode.");
                    _isEnabled = false;
                }
            }
            else
            {
                _logger.LogInformation("RabbitMQ is disabled in configuration. Notifications will be logged only.");
            }
        }

        /// <summary>
        /// Initialize RabbitMQ connection (Sync for RabbitMQ.Client 6.x)
        /// </summary>
        private void InitializeConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost ?? "/",
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare queues (make sure they exist)
                _channel.QueueDeclare(
                    queue: "email.queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _channel.QueueDeclare(
                    queue: "sms.queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _channel.QueueDeclare(
                    queue: "inapp.queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _channel.QueueDeclare(
                    queue: "notifications.queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _logger.LogInformation("RabbitMQ connection established successfully. Host: {HostName}, Port: {Port}",
                    _settings.HostName, _settings.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        /// <summary>
        /// Publish message to RabbitMQ queue
        /// </summary>
        public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                if (_isEnabled && _channel != null)
                {
                    var body = Encoding.UTF8.GetBytes(messageJson);

                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true; // Make message persistent
                    properties.ContentType = "application/json";
                    properties.DeliveryMode = 2; // 2 = Persistent in RabbitMQ 6.x

                    _channel.BasicPublish(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: false,
                        basicProperties: properties,
                        body: body
                    );

                    _logger.LogDebug("Message published to queue '{QueueName}': {Size} bytes", queueName, body.Length);
                }
                else
                {
                    // Fallback: Log the message
                    _logger.LogInformation("RabbitMQ not available. Message would be published to '{QueueName}': {Message}",
                        queueName, messageJson.Substring(0, Math.Min(200, messageJson.Length)));
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to queue '{QueueName}'", queueName);
                // Don't throw - allow application to continue even if messaging fails
            }
        }

        /// <summary>
        /// Check if RabbitMQ is connected and ready
        /// </summary>
        public bool IsConnected()
        {
            return _isEnabled && _connection != null && _connection.IsOpen;
        }

        /// <summary>
        /// Dispose RabbitMQ resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();

                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// RabbitMQ configuration settings
    /// </summary>
    public class RabbitMQSettings
    {
        public bool Enabled { get; set; } = false;
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string? VirtualHost { get; set; } = "/";
    }
}
