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
    [Authorize]
    public async Task<IActionResult> CreateNoteAsync([FromBody] CreateNoteRequestDto createNoteRequestDto, CancellationToken cancellationToken)
    {
        var result = await _noteService.CreateAsync(createNoteRequestDto, cancellationToken);

        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotesAsync([FromQuery] int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var result = await _noteService.GetAllAsync(pageNumber, pageSize, cancellationToken);

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
}