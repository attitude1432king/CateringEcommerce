using CateringEcommerce.Domain.Interfaces.Notification;
using System.Collections.Concurrent;

namespace CateringEcommerce.BAL.Notification
{
    public class RateLimiter : IRateLimiter
    {
        private readonly ConcurrentDictionary<string, SlidingWindowCounter> _counters = new();

        public Task<bool> AllowAsync(string key, int maxRequests, TimeSpan window)
        {
            var counter = _counters.GetOrAdd(key, _ => new SlidingWindowCounter());
            return Task.FromResult(counter.TryIncrement(maxRequests, window));
        }

        public Task ResetAsync(string key)
        {
            _counters.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        private class SlidingWindowCounter
        {
            private readonly object _lock = new();
            private readonly Queue<DateTime> _requestTimes = new();

            public bool TryIncrement(int maxRequests, TimeSpan window)
            {
                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    var cutoff = now - window;

                    // Remove old entries
                    while (_requestTimes.Count > 0 && _requestTimes.Peek() < cutoff)
                    {
                        _requestTimes.Dequeue();
                    }

                    // Check if we can allow this request
                    if (_requestTimes.Count >= maxRequests)
                    {
                        return false;
                    }

                    // Allow the request
                    _requestTimes.Enqueue(now);
                    return true;
                }
            }
        }
    }
}
