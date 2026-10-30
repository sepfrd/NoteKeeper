using NoteKeeper.Shared.Resources;

namespace NoteKeeper.Shared.Results;

public static class Errors
{
    public static readonly Error InternalServerError = new(ErrorCodes.UnknownError, ErrorMessages.InternalServerError);
    public static readonly Error Unauthorized = new(ErrorCodes.Unauthorized, ErrorMessages.Unauthorized);
    public static readonly Error NotFound = new(ErrorCodes.NotFound, ErrorMessages.NotFoundError);
    public static readonly Error BadRequest = new(ErrorCodes.BadRequest, ErrorMessages.BadRequest);
    public static readonly Error Forbidden = new(ErrorCodes.Forbidden, ErrorMessages.Forbidden);
}