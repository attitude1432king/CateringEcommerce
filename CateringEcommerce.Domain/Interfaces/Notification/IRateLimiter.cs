namespace CateringEcommerce.Domain.Interfaces.Notification
{
    public interface IRateLimiter
    {
        Task<bool> AllowAsync(string key, int maxRequests, TimeSpan window);
        Task ResetAsync(string key);
    }
}
