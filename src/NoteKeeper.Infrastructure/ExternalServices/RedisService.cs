using NoteKeeper.Application.Interfaces;
using NoteKeeper.Infrastructure.Interfaces;
using StackExchange.Redis;

namespace NoteKeeper.Infrastructure.ExternalServices;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<RedisValue> GetDeleteStringAsync(string key, int database = 0) =>
        await _connectionMultiplexer
            .GetDatabase(database)
            .StringGetDeleteAsync(key);

    public async Task<RedisValue> GetStringAsync(string key, int database = 0) =>
        await _connectionMultiplexer
            .GetDatabase(database)
            .StringGetAsync(key);

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null, int database = 0) =>
        await _connectionMultiplexer
            .GetDatabase(database)
            .StringSetAsync(key, value, expiry);

    public async Task<bool> DeleteKeyAsync(string key, int database = 0) =>
        await _connectionMultiplexer
            .GetDatabase(database)
            .KeyDeleteAsync(key);

    public async Task<bool> ValueExistsInSetAsync(string redisSetName, string value, int database = 0) =>
        await _connectionMultiplexer
            .GetDatabase(database)
            .SetContainsAsync(redisSetName, value);

    public async Task<bool> AddValueToSetAsync(string redisSetName, string value, int database = 0) =>
        await _connectionMultiplexer
            .GetDatabase(database)
            .SetAddAsync(redisSetName, value);

    public async Task<bool> RemoveValueFromSetAsync(string redisSetName, string value, int database = 0) =>
        await _connectionMultiplexer
            .GetDatabase(database)
            .SetRemoveAsync(redisSetName, value);
}