using StackExchange.Redis;
using System.Text.Json;

namespace Lab789SalesWebApi.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer redis;
        private readonly ILogger<RedisCacheService> logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            this.redis = redis;
            this.logger = logger;
        }

        private IDatabase? GetDatabase()
        {
            try
            {
                if (redis.IsConnected)
                {
                    return redis.GetDatabase();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to get Redis database.");
            }
            return null;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return default;

                var value = await db.StringGetAsync(key);
                if (!value.HasValue) return default;

                return JsonSerializer.Deserialize<T>(value.ToString()!, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis GetAsync error for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return;

                var json = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, json, expiration ?? TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis SetAsync error for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                var db = GetDatabase();
                if (db == null) return;

                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis RemoveAsync error for key: {Key}", key);
            }
        }
    }
}
