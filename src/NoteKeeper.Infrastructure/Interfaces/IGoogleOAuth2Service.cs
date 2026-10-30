using NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;
using NoteKeeper.Shared.Results;

namespace NoteKeeper.Infrastructure.Interfaces;

public interface IGoogleOAuth2Service
{
    Task<DomainResult<string?>> AuthenticateWithGoogleAsync(string redirectUri, CancellationToken cancellationToken = default);

    Task<DomainResult<CompleteGoogleAuthenticationResponseDto?>> CompleteGoogleAuthenticationAsync(
        CompleteGoogleAuthenticationRequestDto requestDto,
        CancellationToken cancellationToken = default);

    Task<DomainResult<string?>> RevokeTokensAsync(CancellationToken cancellationToken);

    Task<DomainResult<string?>> GoogleRefreshAccessTokenAsync(CancellationToken cancellationToken = default);
}