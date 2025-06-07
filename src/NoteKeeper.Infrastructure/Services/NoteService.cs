using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NoteKeeper.Application.Interfaces;
using NoteKeeper.Domain.Entities;
using NoteKeeper.Infrastructure.Common.Constants;
using NoteKeeper.Infrastructure.Common.Dtos;
using NoteKeeper.Infrastructure.Common.Dtos.DomainEntities;
using NoteKeeper.Infrastructure.Common.Dtos.Requests;

namespace NoteKeeper.Infrastructure.Services;

public class NoteService : INoteService
{
    private readonly IRepositoryBase<Note> _noteRepository;
    private readonly IAuthService _authService;
    private readonly IRedisPubSubService<NoteDto> _redisPubSubService;
    private readonly IRedisService _redisService;

    public NoteService(
        IRepositoryBase<Note> noteRepository,
        IAuthService authService,
        IRedisPubSubService<NoteDto> redisPubSubService,
        IRedisService redisService)
    {
        _noteRepository = noteRepository;
        _authService = authService;
        _redisPubSubService = redisPubSubService;
        _redisService = redisService;
    }

    public async Task<ResponseDto<NoteDto?>> CreateAsync(CreateNoteRequestDto createNoteRequestDto, CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(null, cancellationToken);

        if (user is null)
        {
            return new ResponseDto<NoteDto?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        var note = createNoteRequestDto.ToNoteDomainEntity();

        note.UserId = user.Id;

        await _noteRepository.CreateAsync(note, cancellationToken);

        await _noteRepository.SaveChangesAsync(cancellationToken);

        var noteDto = NoteDto.FromNoteDomainEntity(note);

        noteDto.UserUuid = user.Uuid;

        return new ResponseDto<NoteDto?>
        {
            Data = noteDto,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.Created
        };
    }

    public async Task<ResponseDto<PaginatedResult<NoteDto>>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(null, cancellationToken);

        if (user is null)
        {
            return new ResponseDto<PaginatedResult<NoteDto>>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        var paginatedResult = await _noteRepository.GetAllAsync(
            pageNumber,
            pageSize,
            note => note.UserId == user.Id,
            null,
            cancellationToken);

        var noteDtos = paginatedResult.Items.Select(note =>
            {
                var noteDto = NoteDto.FromNoteDomainEntity(note);

                noteDto.UserUuid = user.Uuid;

                return noteDto;
            })
            .ToList();

        return new ResponseDto<PaginatedResult<NoteDto>>
        {
            Data = new PaginatedResult<NoteDto>(
                pageNumber,
                pageSize,
                paginatedResult.TotalCount,
                noteDtos),
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<NoteDto?>> GetByUuidAsync(Guid noteUuid, CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            users => users.Include(user => user.Notes),
            cancellationToken);

        if (user is null)
        {
            return new ResponseDto<NoteDto?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        var note = user.Notes.FirstOrDefault(note => note.Uuid == noteUuid);

        if (note is null)
        {
            var message = string.Format(CultureInfo.InvariantCulture, MessageConstants.EntityNotFoundByGuidMessage, nameof(Note), noteUuid);

            return new ResponseDto<NoteDto?>
            {
                Data = null,
                IsSuccess = false,
                Message = message,
                HttpStatusCode = HttpStatusCode.NotFound
            };
        }

        var noteDto = NoteDto.FromNoteDomainEntity(note);

        noteDto.UserUuid = user.Uuid;

        return new ResponseDto<NoteDto?>
        {
            Data = noteDto,
            IsSuccess = true,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<NoteDto?>> UpdateByUuidAsync(Guid noteUuid, UpdateNoteRequestDto updateNoteRequestDto, CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            users => users.Include(user => user.Notes),
            cancellationToken);

        if (user is null)
        {
            return new ResponseDto<NoteDto?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        var note = user.Notes.FirstOrDefault(note => note.Uuid == noteUuid);

        if (note is null)
        {
            var message = string.Format(CultureInfo.InvariantCulture, MessageConstants.EntityNotFoundByGuidMessage, nameof(Note), noteUuid);

            return new ResponseDto<NoteDto?>
            {
                Data = null,
                IsSuccess = false,
                Message = message,
                HttpStatusCode = HttpStatusCode.NotFound
            };
        }

        note.Title = updateNoteRequestDto.NewTitle;
        note.Content = updateNoteRequestDto.NewContent;
        note.MarkAsUpdated();

        await _noteRepository.SaveChangesAsync(cancellationToken);

        var noteDto = NoteDto.FromNoteDomainEntity(note);

        noteDto.UserUuid = user.Uuid;

        var isSubscribed = await _redisService
            .ValueExistsInSetAsync(
                RedisConstants.NotesSubscriptionSetKey,
                noteUuid.ToString());

        if (isSubscribed)
        {
            await _redisPubSubService.PublishMessageAsync(noteDto, noteUuid.ToString());
        }

        var successMessage = string.Format(CultureInfo.InvariantCulture, MessageConstants.SuccessfulUpdateMessage, nameof(Note).ToLowerInvariant());

        return new ResponseDto<NoteDto?>
        {
            Data = noteDto,
            IsSuccess = true,
            Message = successMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<NoteDto?>> DeleteByUuidAsync(Guid noteUuid, CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            users => users.Include(user => user.Notes),
            cancellationToken);

        if (user is null)
        {
            return new ResponseDto<NoteDto?>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        var note = user.Notes.FirstOrDefault(note => note.Uuid == noteUuid);

        if (note is null)
        {
            var message = string.Format(CultureInfo.InvariantCulture, MessageConstants.EntityNotFoundByGuidMessage, nameof(Note), noteUuid);

            return new ResponseDto<NoteDto?>
            {
                Data = null,
                IsSuccess = false,
                Message = message,
                HttpStatusCode = HttpStatusCode.NotFound
            };
        }

        var deletedNote = _noteRepository.Delete(note);

        await _noteRepository.SaveChangesAsync(cancellationToken);

        var noteDto = NoteDto.FromNoteDomainEntity(deletedNote);

        noteDto.UserUuid = user.Uuid;

        var successMessage = string.Format(CultureInfo.InvariantCulture, MessageConstants.SuccessfulDeleteMessage, nameof(Note).ToLowerInvariant());

        return new ResponseDto<NoteDto?>
        {
            Data = noteDto,
            IsSuccess = true,
            Message = successMessage,
            HttpStatusCode = HttpStatusCode.OK
        };
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
}