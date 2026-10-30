namespace NoteKeeper.Shared.Results;

public static class ErrorCodes
{
    public const string None = nameof(None);

    // General
    public const string UnknownError = nameof(UnknownError);
    public const string InvalidOperation = nameof(InvalidOperation);
    public const string NotImplemented = nameof(NotImplemented);

    // Validation
    public const string ValidationFailed = nameof(ValidationFailed);
    public const string RequiredFieldMissing = nameof(RequiredFieldMissing);
    public const string OutOfRange = nameof(OutOfRange);
    public const string DuplicateEntry = nameof(DuplicateEntry);

    // CRUD / Data
    public const string NotFound = nameof(NotFound);
    public const string InvalidData = nameof(InvalidData);
    public const string AlreadyExists = nameof(AlreadyExists);
    public const string IdenticalValues = nameof(IdenticalValues);
    public const string Conflict = nameof(Conflict);
    public const string BadRequest = nameof(BadRequest);

    // Authentication / Authorization
    public const string Unauthorized = nameof(Unauthorized);
    public const string Forbidden = nameof(Forbidden);
    public const string InvalidCredentials = nameof(InvalidCredentials);
    public const string TokenExpired = nameof(TokenExpired);
    public const string TokenInvalid = nameof(TokenInvalid);
}