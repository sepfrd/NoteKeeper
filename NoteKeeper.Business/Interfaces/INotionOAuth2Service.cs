using NoteKeeper.Business.Dtos;
using NoteKeeper.Business.Dtos.Notion;

namespace NoteKeeper.Business.Interfaces;

public interface INotionOAuth2Service
{
    Task<ResponseDto<string?>> UseNotionOAuth2Async(CancellationToken cancellationToken = default);

    Task<ResponseDto<string?>> NotionExchangeCodeForTokensAsync(NotionExchangeCodeForTokenRequestDto exchangeCodeForTokenRequestDto, CancellationToken cancellationToken = default);
}