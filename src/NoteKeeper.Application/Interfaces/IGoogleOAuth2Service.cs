namespace NoteKeeper.Application.Interfaces;

public interface IGoogleOAuth2Service
{
    Task<ResponseDto<string?>> AuthenticateWithGoogleAsync(CancellationToken cancellationToken = default);

    Task<ResponseDto<string?>> CompleteGoogleAuthenticationAsync(CompleteGoogleAuthenticationAsyncRequestDto requestDto, CancellationToken cancellationToken = default);

    Task<ResponseDto<string?>> RevokeTokensAsync(CancellationToken cancellationToken);

    Task<ResponseDto<string?>> GoogleRefreshAccessTokenAsync(CancellationToken cancellationToken = default);
}