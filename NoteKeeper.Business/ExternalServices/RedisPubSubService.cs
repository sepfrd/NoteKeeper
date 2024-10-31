using System.Text.Json;
using NoteKeeper.Business.Interfaces;
using StackExchange.Redis;

namespace NoteKeeper.Business.ExternalServices;

public class RedisPubSubService<T> : IRedisPubSubService<T> where T : class
{
    private readonly ISubscriber _redisSubscriber;

    public RedisPubSubService(IConnectionMultiplexer connectionMultiplexer)
    {
        _redisSubscriber = connectionMultiplexer.GetSubscriber();
    }

    public async Task PublishMessageAsync(T message, string channel)
    {
        var serializedMessage = JsonSerializer.Serialize(message);

        await _redisSubscriber.PublishAsync(channel, serializedMessage);
    }

    public async Task SubscribeToChannelAsync(string channel, Action<RedisChannel, RedisValue> handler) =>
        await _redisSubscriber.SubscribeAsync(channel, handler);

    public async Task UnsubscribeFromChannelAsync(string channel, Action<RedisChannel, RedisValue> handler) =>
        await _redisSubscriber.UnsubscribeAsync(channel, handler);
}