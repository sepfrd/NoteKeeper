using NoteKeeper.Domain.Common;
using NoteKeeper.Infrastructure.Common.Dtos.Google;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IGoogleOAuth2Service
{
    Task<DomainResult<string?>> AuthenticateWithGoogleAsync(CancellationToken cancellationToken = default);

    Task<DomainResult<string?>> CompleteGoogleAuthenticationAsync(CompleteGoogleAuthenticationAsyncRequestDto requestDto, CancellationToken cancellationToken = default);

    Task<DomainResult<string?>> RevokeTokensAsync(CancellationToken cancellationToken);

    Task<DomainResult<string?>> GoogleRefreshAccessTokenAsync(CancellationToken cancellationToken = default);
}