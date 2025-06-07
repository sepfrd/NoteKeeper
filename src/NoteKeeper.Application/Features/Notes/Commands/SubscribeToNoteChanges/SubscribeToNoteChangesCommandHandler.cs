using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;

namespace NoteKeeper.Application.Features.Notes.Commands.SubscribeToNoteChanges;

public class SubscribeToNoteChangesCommandHandler : ICommandHandler<SubscribeToNoteChangesCommand, DomainResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IRedisPubSubService<NoteDto> _redisPubSubService;
    private readonly IRedisService _redisService;

    public SubscribeToNoteChangesCommandHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        IRedisPubSubService<NoteDto> redisPubSubService,
        IRedisService redisService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
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
    }

    public Task<DomainResult> HandleAsync(SubscribeToNoteChangesCommand command, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}