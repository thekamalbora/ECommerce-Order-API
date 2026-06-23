using ECommerce.API.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ECommerce.API.Helpers
{
    public interface ICacheService
    {
        Task ClearProductCache();
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        private readonly IConnectionMultiplexer _redis;

        public CacheService(IDistributedCache cache, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _redis = redis;
        }

        public async Task ClearProductCache()
        {
            var endpoint = _redis.GetEndPoints().First();

            var server = _redis.GetServer(endpoint);

            var keys = server.Keys(pattern: "ECommerce:products*");

            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key.ToString());
            }
        }
    }
}