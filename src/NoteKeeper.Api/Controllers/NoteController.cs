using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteKeeper.Application.Features.Notes.Commands.CreateNote;
using NoteKeeper.Application.Features.Notes.Commands.DeleteByUuid;
using NoteKeeper.Application.Features.Notes.Commands.UpdateByUuid;
using NoteKeeper.Application.Features.Notes.Dtos;
using NoteKeeper.Application.Features.Notes.Queries.GetAllNotes;
using NoteKeeper.Application.Features.Notes.Queries.GetAllNotesCount;
using NoteKeeper.Application.Features.Notes.Queries.GetNoteByUuid;
using NoteKeeper.Application.Interfaces.CQRS;
using NoteKeeper.Domain.Common;
using NoteKeeper.Infrastructure.Common.Dtos.Requests;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notes")]
public class NoteController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<CreateNoteRequestDto> _createNoteRequestDtoValidator;
    private readonly IValidator<UpdateNoteRequestDto> _updateNoteRequestDtoValidator;
    private readonly ICommandHandler<CreateNoteCommand, DomainResult<NoteDto>> _createNoteCommandHandler;
    private readonly ICommandHandler<DeleteNoteCommand, DomainResult> _deleteNoteCommandHandler;
    private readonly ICommandHandler<UpdateNoteCommand, DomainResult<NoteDto>> _updateNoteCommandHandler;
    private readonly IQueryHandler<GetAllNotesByFilterQuery, PaginatedDomainResult<IEnumerable<NoteDto>>> _getAllNotesByFilterQueryHandler;
    private readonly IQueryHandler<GetAllNotesCountQuery, DomainResult<long>> _getAllNotesCountQueryHandler;
    private readonly IQueryHandler<GetNoteByUuidQuery, DomainResult<NoteDto>> _getNoteByUuidQueryHandler;

    public NoteController(
        IAuthService authService,
        IValidator<CreateNoteRequestDto> createNoteRequestDtoValidator,
        IValidator<UpdateNoteRequestDto> updateNoteRequestDtoValidator,
        ICommandHandler<CreateNoteCommand, DomainResult<NoteDto>> createNoteCommandHandler,
        ICommandHandler<DeleteNoteCommand, DomainResult> deleteNoteCommandHandler,
        ICommandHandler<UpdateNoteCommand, DomainResult<NoteDto>> updateNoteCommandHandler,
        IQueryHandler<GetAllNotesByFilterQuery, PaginatedDomainResult<IEnumerable<NoteDto>>> getAllNotesByFilterQueryHandler,
        IQueryHandler<GetAllNotesCountQuery, DomainResult<long>> getAllNotesCountQueryHandler,
        IQueryHandler<GetNoteByUuidQuery, DomainResult<NoteDto>> getNoteByUuidQueryHandler)
    {
        _createNoteCommandHandler = createNoteCommandHandler;
        _deleteNoteCommandHandler = deleteNoteCommandHandler;
        _updateNoteCommandHandler = updateNoteCommandHandler;
        _getAllNotesByFilterQueryHandler = getAllNotesByFilterQueryHandler;
        _getAllNotesCountQueryHandler = getAllNotesCountQueryHandler;
        _getNoteByUuidQueryHandler = getNoteByUuidQueryHandler;
        _createNoteRequestDtoValidator = createNoteRequestDtoValidator;
        _updateNoteRequestDtoValidator = updateNoteRequestDtoValidator;
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNoteAsync([FromBody] CreateNoteRequestDto createNoteRequestDto, CancellationToken cancellationToken)
    {
        var validationResult = await _createNoteRequestDtoValidator.ValidateAsync(createNoteRequestDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(DomainResult.CreateBaseFailure(validationResult.ToString(), StatusCodes.Status400BadRequest));
        }

        var signedInUserUuid = _authService.GetSignedInUserUuid();

        var command = new CreateNoteCommand(
            createNoteRequestDto.Title,
            createNoteRequestDto.Content,
            Guid.Parse(signedInUserUuid));

        var result = await _createNoteCommandHandler.HandleAsync(command, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotesAsync(
        [FromQuery] uint? pageNumber,
        [FromQuery] uint? pageSize,
        [FromQuery] NoteFilterDto? noteFilterDto,
        CancellationToken cancellationToken)
    {
        var signedInUserUuid = _authService.GetSignedInUserUuid();

        noteFilterDto ??= new NoteFilterDto();

        noteFilterDto.UserUuid = Guid.Parse(signedInUserUuid);

        var query = new GetAllNotesByFilterQuery(noteFilterDto, pageNumber ?? 1, pageSize ?? 10);

        var result = await _getAllNotesByFilterQueryHandler.HandleAsync(query, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    [Route("uuid/{noteUuid:guid}")]
    public async Task<IActionResult> GetNoteByUuidAsync([FromRoute] Guid noteUuid, CancellationToken cancellationToken)
    {
        var query = new GetNoteByUuidQuery(noteUuid);

        var result = await _getNoteByUuidQueryHandler.HandleAsync(query, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch]
    [Route("uuid/{noteUuid:guid}")]
    public async Task<IActionResult> UpdateNoteByUuidAsync(
        [FromRoute] Guid noteUuid,
        UpdateNoteRequestDto updateNoteRequestDto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateNoteRequestDtoValidator.ValidateAsync(updateNoteRequestDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(DomainResult.CreateBaseFailure(validationResult.ToString(), StatusCodes.Status400BadRequest));
        }

        var signedInUserUuid = _authService.GetSignedInUserUuid();

        var command = new UpdateNoteCommand(
            noteUuid,
            Guid.Parse(signedInUserUuid),
            updateNoteRequestDto.NewTitle,
            updateNoteRequestDto.NewContent);

        var result = await _updateNoteCommandHandler.HandleAsync(command, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete]
    [Route("uuid/{noteUuid:guid}")]
    public async Task<IActionResult> DeleteNoteByUuidAsync([FromRoute] Guid noteUuid, CancellationToken cancellationToken)
    {
        var signedInUserUuid = _authService.GetSignedInUserUuid();

        var command = new DeleteNoteCommand(
            noteUuid,
            Guid.Parse(signedInUserUuid));

        var result = await _deleteNoteCommandHandler.HandleAsync(command, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }
}