using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using NoteKeeper.Business.Constants;
using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Dtos.DomainEntities;
using NoteKeeper.Business.Dtos.Requests;
using NoteKeeper.Business.Interfaces;
using NoteKeeper.DataAccess.Entities;
using NoteKeeper.DataAccess.Interfaces;

namespace NoteKeeper.Business.Services;

public class NoteService : INoteService
{
    private readonly IRepositoryBase<Note> _noteRepository;
    private readonly IAuthService _authService;

    public NoteService(IRepositoryBase<Note> noteRepository, IAuthService authService)
    {
        _noteRepository = noteRepository;
        _authService = authService;
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

    public async Task<ResponseDto<List<NoteDto>>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetSignedInUserAsync(
            users => users.Include(user => user.Notes),
            cancellationToken);

        if (user is null)
        {
            return new ResponseDto<List<NoteDto>>
            {
                IsSuccess = false,
                HttpStatusCode = HttpStatusCode.Unauthorized
            };
        }

        var skipCount = (pageNumber - 1) * pageSize;

        var notes = user
            .Notes
            .OrderByDescending(note => note.Id)
            .Skip(skipCount)
            .Take(pageSize).ToList();

        var response = notes.Select(note =>
            {
                var noteDto = NoteDto.FromNoteDomainEntity(note);

                noteDto.UserUuid = user.Uuid;

                return noteDto;
            })
            .ToList();

        return new ResponseDto<List<NoteDto>>
        {
            Data = response,
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
}