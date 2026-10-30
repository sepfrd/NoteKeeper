namespace NoteKeeper.Shared.Results;

public record DomainResult
{
    protected DomainResult(int statusCode, string? message = null, Error? error = null)
    {
        Message = message;
        Error = error;
        Success = statusCode >= 200 && statusCode < 300;
        StatusCode = statusCode;
    }

    public bool Success { get; init; }

    public string? Message { get; }

    public Error? Error { get; init; }

    public int StatusCode { get; set; }

    public static DomainResult CreateBaseSuccess(string message, int statusCode) => new(statusCode, message);

    public static DomainResult CreateBaseFailure(Error error, int statusCode, string? message = null) =>
        new(statusCode, message, error);
}

public record DomainResult<T> : DomainResult
{
    protected DomainResult(T? data, bool success, int statusCode, string? message = null, Error? error = null)
        : base(statusCode, message, error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public T? Data { get; init; }

    public static DomainResult<T> CreateSuccess(T value, int statusCode, string? message = null) =>
        new(value, true, statusCode, message);

    public static DomainResult<T> CreateFailure(Error error, int statusCode, string? message = null) =>
        new(default, false, statusCode, message, error);
}