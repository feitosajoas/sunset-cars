using Microsoft.Extensions.Caching.Memory;
using SunsetCars.Models;

namespace SunsetCars.Services
{
    public interface ICacheService
    {
        Task<List<T>> GetOrCreateAsync<T>(string key, Func<Task<List<T>>> factory, TimeSpan? expiration = null);
        Task<T?> GetOrCreateSingleAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class;
        void Remove(string key);
        Task RemoveAsync(string key);
        void RemoveByPrefix(string prefix);
        Task RemoveByPrefixAsync(string prefix);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _cacheKeys;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
            _cacheKeys = new HashSet<string>();
        }

        public async Task<List<T>> GetOrCreateAsync<T>(string key, Func<Task<List<T>>> factory, TimeSpan? expiration = null)
        {
            if (!_cache.TryGetValue(key, out List<T>? cachedValue))
            {
                cachedValue = await factory();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(30));

                _cache.Set(key, cachedValue, cacheEntryOptions);
                _cacheKeys.Add(key);
            }

            return cachedValue ?? new List<T>();
        }

        public async Task<T?> GetOrCreateSingleAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class
        {
            if (!_cache.TryGetValue(key, out T? cachedValue))
            {
                cachedValue = await factory();

                if (cachedValue != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(30));

                    _cache.Set(key, cachedValue, cacheEntryOptions);
                    _cacheKeys.Add(key);
                }
            }

            return cachedValue;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            _cacheKeys.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void RemoveByPrefix(string prefix)
        {
            var keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _cacheKeys.Remove(key);
            }
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            RemoveByPrefix(prefix);
            return Task.CompletedTask;
        }
    }
}
