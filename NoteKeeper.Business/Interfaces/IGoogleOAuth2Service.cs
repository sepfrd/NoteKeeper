using NoteKeeper.Business.Dtos;

namespace NoteKeeper.Business.Interfaces;

public interface IGoogleOAuth2Service
{
    Task<ResponseDto<string?>> UseGoogleOAuth2Async(CancellationToken cancellationToken = default);

    Task<ResponseDto<string?>> GoogleExchangeCodeForTokenAsync(GoogleExchangeCodeForTokenRequestDto exchangeCodeForTokenRequestDto, CancellationToken cancellationToken = default);
}