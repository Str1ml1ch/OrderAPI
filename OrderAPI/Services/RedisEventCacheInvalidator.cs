using Microsoft.Extensions.Caching.Distributed;
using OrderAPI.Domain.Services;

namespace OrderAPI.Services
{
    public class RedisEventCacheInvalidator : IEventCacheInvalidator
    {
        private const string CacheVersionKey = "events:cache:version";
        private static readonly DistributedCacheEntryOptions Options = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        };

        private readonly IDistributedCache _cache;

        public RedisEventCacheInvalidator(IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task InvalidateAsync(CancellationToken cancellationToken)
            => _cache.SetStringAsync(CacheVersionKey, Guid.NewGuid().ToString(), Options, cancellationToken);
    }
}
