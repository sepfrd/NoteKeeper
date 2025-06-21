namespace NoteKeeper.Domain.Common;

public record DomainResult
{
    protected DomainResult(string? message, int statusCode)
    {
        Message = message;
        StatusCode = statusCode;
        IsSuccess = statusCode >= 200 && statusCode < 400;
    }

    public string? Message { get; init; }

    public int StatusCode { get; init; }

    public bool IsSuccess { get; init; }

    public static DomainResult CreateBaseSuccess(string? message, int resultCode) =>
        new(message, resultCode);

    public static DomainResult CreateBaseFailure(string message, int resultCode) =>
        new(message, resultCode);
}

public record DomainResult<T> : DomainResult
{
    protected DomainResult(string? message, int statusCode, T? data) : base(message, statusCode)
    {
        Data = data;
    }

    public T? Data { get; init; }

    public static DomainResult<T> CreateSuccess(string? message, int resultCode, T data) =>
        new(message, resultCode, data);

    public static DomainResult<T> CreateFailure(string message, int resultCode) =>
        new(message, resultCode, default);
}