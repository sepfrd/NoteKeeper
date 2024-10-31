using NoteKeeper.Business.Interfaces;
using StackExchange.Redis;

namespace NoteKeeper.Business.ExternalServices;

public class RedisService : IRedisService
{
    private readonly IDatabase _redisDatabase;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _redisDatabase = connectionMultiplexer.GetDatabase();
    }

    public async Task<bool> ValueExistsInRedisSetAsync(string redisSetName, string value) =>
        await _redisDatabase.SetContainsAsync(redisSetName, value);

    public async Task<bool> AddValueToRedisSetAsync(string redisSetName, string value) =>
        await _redisDatabase.SetAddAsync(redisSetName, value);

    public async Task<bool> RemoveValueFromRedisSetAsync(string redisSetName, string value) =>
        await _redisDatabase.SetRemoveAsync(redisSetName, value);
}