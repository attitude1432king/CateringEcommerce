# RabbitMQ Setup Guide for Notification System

## Overview

The notification system can optionally use RabbitMQ for asynchronous message processing. This guide explains how to set up and configure RabbitMQ.

---

## 🚀 Quick Start (Optional)

**Note**: RabbitMQ is **optional**. The notification system will work without it by logging notifications instead of queuing them.

---

## 📦 Installation Options

### Option 1: Docker (Recommended for Development)

```bash
# Pull RabbitMQ image with management plugin
docker pull rabbitmq:3-management

# Run RabbitMQ container
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=admin \
  -e RABBITMQ_DEFAULT_PASS=admin123 \
  rabbitmq:3-management
```

**Management UI**: http://localhost:15672 (admin/admin123)

### Option 2: Windows Installation

1. Download Erlang: https://www.erlang.org/downloads
2. Download RabbitMQ: https://www.rabbitmq.com/download.html
3. Install both in order (Erlang first, then RabbitMQ)
4. Enable management plugin:
   ```cmd
   rabbitmq-plugins enable rabbitmq_management
   ```

### Option 3: Cloud (Production)

- **CloudAMQP**: https://www.cloudamqp.com/ (Free tier available)
- **AWS MQ**: https://aws.amazon.com/amazon-mq/
- **Azure Service Bus**: https://azure.microsoft.com/services/service-bus/

---

## ⚙️ Configuration

### 1. Install RabbitMQ.Client NuGet Package

```bash
cd CateringEcommerce.BAL
dotnet add package RabbitMQ.Client
```

### 2. Update `appsettings.json`

Add RabbitMQ configuration:

```json
{
  "RabbitMQ": {
    "Enabled": true,
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "admin",
    "Password": "admin123",
    "VirtualHost": "/"
  }
}
```

**For Production (CloudAMQP example)**:
```json
{
  "RabbitMQ": {
    "Enabled": true,
    "HostName": "your-instance.cloudamqp.com",
    "Port": 5672,
    "UserName": "your-username",
    "Password": "your-password",
    "VirtualHost": "your-vhost"
  }
}
```

### 3. Uncomment RabbitMQ Code

In `CateringEcommerce.BAL/Services/RabbitMQPublisher.cs`:

1. Uncomment the using statement at the top:
   ```csharp
   using RabbitMQ.Client;
   ```

2. Uncomment the connection initialization code in `InitializeConnection()` method

3. Uncomment the publishing code in `PublishAsync()` method

4. Uncomment the disposal code in `Dispose()` method

### 4. Register RabbitMQ in Program.cs (Optional)

```csharp
// Add RabbitMQ settings
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// Register RabbitMQ Publisher as singleton
builder.Services.AddSingleton<RabbitMQPublisher>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMQPublisher>>();
    var settings = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
    return new RabbitMQPublisher(logger, settings);
});

// Update NotificationHelper to use RabbitMQ
builder.Services.AddScoped<NotificationHelper>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<NotificationHelper>>();
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("DefaultConnection");
    var rabbitMQ = sp.GetRequiredService<RabbitMQPublisher>();

    return new NotificationHelper(logger, connStr, "http://localhost:5000", rabbitMQ);
});
```

---

## 📋 Queue Structure

The notification system uses the following queues:

| Queue Name | Purpose | Durability |
|------------|---------|------------|
| `email.queue` | Email notifications | Durable |
| `sms.queue` | SMS notifications | Durable |
| `inapp.queue` | In-app notifications | Durable |
| `notifications.queue` | Fallback/general notifications | Durable |

**Queues are auto-created** when the application starts (if RabbitMQ is enabled).

---

## 🔧 Consumer Setup (Future Work)

To process notifications from queues, you'll need to create consumer services:

### Email Consumer Example

```csharp
public class EmailConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IEmailService _emailService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var notification = JsonSerializer.Deserialize<NotificationMessage>(message);

            // Send email
            await _emailService.SendEmailAsync(notification);

            // Acknowledge message
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: "email.queue", autoAck: false, consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
```

Register in Program.cs:
```csharp
builder.Services.AddHostedService<EmailConsumerService>();
builder.Services.AddHostedService<SmsConsumerService>();
builder.Services.AddHostedService<InAppConsumerService>();
```

---

## 🧪 Testing RabbitMQ

### 1. Check Connection

```bash
# Docker
docker logs rabbitmq

# Check queues via management UI
http://localhost:15672/#/queues
```

### 2. Manual Queue Inspection

Using RabbitMQ Management UI:
1. Go to http://localhost:15672
2. Login (admin/admin123)
3. Click "Queues" tab
4. You should see: `email.queue`, `sms.queue`, `inapp.queue`
5. Click on a queue to see messages

### 3. Test Notification

Create a test endpoint:
```csharp
[HttpPost("test-notification")]
public async Task<IActionResult> TestNotification()
{
    var notificationHelper = new NotificationHelper(_logger, _connStr, "http://localhost:5000", _rabbitMQPublisher);

    await notificationHelper.SendMultiChannelNotificationAsync(
        "TEST_NOTIFICATION",
        "USER",
        "123",
        "test@example.com",
        "+911234567890",
        new Dictionary<string, object> { { "test_key", "test_value" } }
    );

    return Ok("Notification sent to queue!");
}
```

---

## 🔍 Monitoring

### RabbitMQ Management Dashboard

- **URL**: http://localhost:15672
- **Features**:
  - Queue statistics (messages, consumers, rates)
  - Connection monitoring
  - Channel details
  - Message acknowledgment rates
  - Memory usage

### Key Metrics to Monitor

1. **Message Rate**: Messages/second published and consumed
2. **Queue Depth**: Number of unprocessed messages
3. **Consumer Count**: Number of active consumers per queue
4. **Acknowledgment Rate**: Messages successfully processed
5. **Error Rate**: Failed message processing

---

## 🚨 Troubleshooting

### Issue: Connection Refused

**Solution**:
```bash
# Check if RabbitMQ is running
docker ps | grep rabbitmq

# Restart RabbitMQ
docker restart rabbitmq
```

### Issue: Messages Not Being Consumed

**Solution**:
1. Check consumer services are running
2. Verify queue names match exactly
3. Check for consumer errors in logs
4. Verify network connectivity

### Issue: High Memory Usage

**Solution**:
1. Set message TTL (Time To Live)
2. Configure queue length limits
3. Increase consumer count
4. Enable lazy queues for large backlogs

---

## 🔒 Production Best Practices

1. **Use Separate Clusters**: Don't mix dev/prod queues
2. **Enable TLS**: Encrypt connections in production
3. **Set Resource Limits**: Configure memory and disk alarms
4. **Monitor Queues**: Set up alerts for queue depth
5. **Backup Configuration**: Export queue/exchange definitions
6. **Use Prefetch Count**: Limit messages per consumer
7. **Dead Letter Queues**: Handle failed messages
8. **Message Persistence**: Ensure durable queues and persistent messages

---

## ⏭️ Without RabbitMQ

If you choose **not** to use RabbitMQ:

1. Set `"Enabled": false` in appsettings.json
2. Notifications will be **logged only**
3. No async processing
4. Direct synchronous notification sending (if implemented)

The system is designed to work in **fallback mode** without RabbitMQ.

---

## 📚 Additional Resources

- [RabbitMQ Official Docs](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ .NET Client Guide](https://www.rabbitmq.com/dotnet-api-guide.html)
- [CloudAMQP Best Practices](https://www.cloudamqp.com/blog/part1-rabbitmq-best-practice.html)
- [RabbitMQ in Production](https://www.rabbitmq.com/production-checklist.html)

---

## ✅ Checklist

- [ ] RabbitMQ installed (Docker/Local/Cloud)
- [ ] RabbitMQ.Client NuGet package installed
- [ ] Configuration added to appsettings.json
- [ ] RabbitMQPublisher.cs code uncommented
- [ ] RabbitMQ registered in Program.cs (optional)
- [ ] Consumer services created (optional)
- [ ] Management UI accessible
- [ ] Test notification sent successfully
- [ ] Monitoring set up

---

**Status**: RabbitMQ integration is **ready** but **optional**. The notification system works without it.
