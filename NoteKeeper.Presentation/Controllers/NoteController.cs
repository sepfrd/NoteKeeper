using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteKeeper.Business.Dtos.Requests;
using NoteKeeper.Business.Interfaces;

namespace NoteKeeper.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/notes")]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNoteAsync([FromBody] CreateNoteRequestDto createNoteRequestDto, CancellationToken cancellationToken)
    {
        var result = await _noteService.CreateAsync(createNoteRequestDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    [Route("uuid/{noteUuid:guid}/subscription")]
    public async Task<IActionResult> SubscribeToNoteChangesAsync([FromRoute] Guid noteUuid, CancellationToken cancellationToken)
    {
        await _noteService.SubscribeToNoteChangesAsync(noteUuid);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotesAsync([FromQuery] int? pageNumber, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var result = await _noteService.GetAllAsync(pageNumber ?? 1, pageSize ?? 10, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet]
    [Route("uuid/{noteUuid:guid}")]
    public async Task<IActionResult> GetNoteByUuidAsync([FromRoute] Guid noteUuid, CancellationToken cancellationToken)
    {
        var result = await _noteService.GetByUuidAsync(noteUuid, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPatch]
    [Route("uuid/{noteUuid:guid}")]
    public async Task<IActionResult> UpdateNoteByUuidAsync(
        [FromRoute] Guid noteUuid,
        UpdateNoteRequestDto updateNoteRequestDto,
        CancellationToken cancellationToken)
    {
        var result = await _noteService.UpdateByUuidAsync(
            noteUuid,
            updateNoteRequestDto,
            cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete]
    [Route("uuid/{noteUuid:guid}")]
    public async Task<IActionResult> DeleteNoteByUuidAsync([FromRoute] Guid noteUuid, CancellationToken cancellationToken)
    {
        var result = await _noteService.DeleteByUuidAsync(noteUuid, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete]
    [Route("uuid/{noteUuid:guid}/subscription")]
    public async Task<IActionResult> UnsubscribeFromNoteChangesAsync([FromRoute] Guid noteUuid, CancellationToken cancellationToken)
    {
        await _noteService.UnsubscribeFromNoteChangesAsync(noteUuid);

        return Ok();
    }
}