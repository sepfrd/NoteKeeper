using System.Globalization;
using System.Text.Json;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Interfaces;
using StackExchange.Redis;

namespace NoteKeeper.Infrastructure.Services;

public class NoteService : INoteService
{
    private readonly IRedisPubSubService<NoteDto> _redisPubSubService;
    private readonly IRedisService _redisService;

    public NoteService(
        IRedisPubSubService<NoteDto> redisPubSubService,
        IRedisService redisService)
    {
        _redisPubSubService = redisPubSubService;
        _redisService = redisService;
    }

    public async Task SubscribeToNoteChangesAsync(Guid noteUuid)
    {
        var noteUuidString = noteUuid.ToString();

        await _redisPubSubService.SubscribeToChannelAsync(noteUuidString, SubscriptionHandler);

        await _redisService.AddValueToSetAsync(RedisConstants.NotesSubscriptionSetKey, noteUuidString);
    }

    public async Task UnsubscribeFromNoteChangesAsync(Guid noteUuid)
    {
        var noteUuidString = noteUuid.ToString();

        await _redisPubSubService.UnsubscribeFromChannelAsync(noteUuidString, SubscriptionHandler);

        await _redisService.RemoveValueFromSetAsync(RedisConstants.NotesSubscriptionSetKey, noteUuidString);
    }

    private static void SubscriptionHandler(RedisChannel redisChannel, RedisValue redisValue)
    {
        const string messageTemplate = "Received a Redis message from channel '{0}': \n{1}";

        Console.ForegroundColor = ConsoleColor.Cyan;

        var noteDto = JsonSerializer.Deserialize<NoteDto>(redisValue!);

        var serializedRedisValue = JsonSerializer.Serialize(noteDto);

        var message = string.Format(CultureInfo.InvariantCulture, messageTemplate, redisChannel, serializedRedisValue);

        Console.WriteLine(message);

        Console.ResetColor();
    }
}