using EarnTrackerApi.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EarnTrackerApi.Services;

public sealed class CacheService(IMemoryCache cache) : ICacheService
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory(cancellationToken);
        cache.Set(key, value, lifetime);
        return value;
    }

    public void Remove(string key)
    {
        cache.Remove(key);
    }
}
