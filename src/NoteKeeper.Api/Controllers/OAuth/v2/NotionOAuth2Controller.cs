using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Data;
using NoteKeeper.Infrastructure.Interfaces;

namespace NoteKeeper.Api.Controllers.OAuth.v2;

[Route("api/oauth/v2/notion")]
public class NotionOAuth2Controller : ControllerBase
{
    private readonly INotionOAuth2Service _notionOAuth2Service;

    public NotionOAuth2Controller(INotionOAuth2Service notionOAuth2Service)
    {
        _notionOAuth2Service = notionOAuth2Service;
    }

    [HttpPost]
    [Route("connect")]
    public async Task<IActionResult> UseNotionOAuth2Async(CancellationToken cancellationToken)
    {
        var result = await _notionOAuth2Service.UseNotionOAuth2Async(cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    [Route("code")]
    public async Task<IActionResult> ReceiveNotionAuthorizationCode(
        [FromQuery] string state,
        [FromQuery] string code,
        [FromQuery] string error,
        CancellationToken cancellationToken)
    {
        var exchangeRequestDto = new NotionExchangeCodeForTokenRequestDto(state, code, error);

        var result = await _notionOAuth2Service.NotionExchangeCodeForTokensAsync(exchangeRequestDto, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpOptions]
    public IActionResult NotionOAuth2Options()
    {
        Response
            .Headers
            .Add(new KeyValuePair<string, StringValues>("Allow", $"{HttpMethods.Post},{HttpMethods.Get}"));

        return Ok();
    }
}