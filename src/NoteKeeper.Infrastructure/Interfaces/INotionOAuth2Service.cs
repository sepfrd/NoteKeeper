using NoteKeeper.Domain.Common;
using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Notion.Data;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface INotionOAuth2Service
{
    Task<DomainResult<string?>> UseNotionOAuth2Async(CancellationToken cancellationToken = default);

    Task<DomainResult<string?>> NotionExchangeCodeForTokensAsync(NotionExchangeCodeForTokenRequestDto exchangeCodeForTokenRequestDto, CancellationToken cancellationToken = default);
}