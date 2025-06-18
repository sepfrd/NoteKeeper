using NoteKeeper.Infrastructure.Common.Dtos.Responses;

namespace NoteKeeper.Infrastructure.ExternalServices.OAuth.V2.Google.Models;

public record CompleteGoogleAuthenticationResponseDto(AuthResponseDto AuthResponseDto, string RedirectUri);