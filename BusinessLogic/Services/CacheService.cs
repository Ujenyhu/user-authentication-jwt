using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using userauthjwt.BusinessLogic.Interfaces;

namespace userauthjwt.BusinessLogic.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private const int defaultCacheDuration = 30;

        public CacheService(IDistributedCache cache) => _cache = cache;

        public async Task<T?> GetAsync<T>(string key)
        {
            var cached = await _cache.GetStringAsync(key);
            if (cached == null)
                return default;

            return JsonSerializer.Deserialize<T>(cached);
        }

        public async void SetAsync<T>(string key, T value, TimeSpan? expiration)
        {
            var cacheData = JsonSerializer.Serialize(value);

            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(defaultCacheDuration)
            };
            await _cache.SetStringAsync(key, cacheData, cacheEntryOptions);
        }

        public async void RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key); 
        }

    }
}
