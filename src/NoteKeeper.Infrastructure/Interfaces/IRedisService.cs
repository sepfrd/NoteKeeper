using StackExchange.Redis;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IRedisService
{
    Task<RedisValue> GetDeleteStringAsync(string key, int database = 0);

    Task<RedisValue> GetStringAsync(string key, int database = 0);

    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, int database = 0);

    Task<bool> DeleteKeyAsync(string key, int database = 0);
}