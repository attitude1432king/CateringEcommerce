namespace CateringEcommerce.Domain.Models.Notification
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "notifications";
        public bool Durable { get; set; } = true;
        public int PrefetchCount { get; set; } = 10;
    }
}
