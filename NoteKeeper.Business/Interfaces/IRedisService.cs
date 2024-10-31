namespace NoteKeeper.Business.Interfaces;

public interface IRedisService
{
    Task<bool> ValueExistsInRedisSetAsync(string redisSetName, string value);

    Task<bool> AddValueToRedisSetAsync(string redisSetName, string value);

    Task<bool> RemoveValueFromRedisSetAsync(string redisSetName, string value);
}