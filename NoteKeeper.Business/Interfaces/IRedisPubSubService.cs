using StackExchange.Redis;

namespace NoteKeeper.Business.Interfaces;

public interface IRedisPubSubService<in T> where T : class
{
    Task PublishMessageAsync(T message, string channel);

    Task SubscribeToChannelAsync(string channel, Action<RedisChannel, RedisValue> handler);

    Task UnsubscribeFromChannelAsync(string channel, Action<RedisChannel, RedisValue> handler);
}