namespace NoteKeeper.Shared.Results;

public record PaginatedDomainResult<T> : DomainResult where T : class, IEnumerable<object>
{
    protected PaginatedDomainResult(
        string? message,
        Error? error,
        T? data,
        int statusCode,
        uint pageNumber,
        uint pageSize,
        uint totalCount)
        : base(statusCode, message, error)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        CurrentPageCount = data is null ? 0 : (uint)data.Count();
        TotalCount = totalCount;
    }

    public uint PageNumber { get; init; }

    public uint PageSize { get; init; }

    public uint CurrentPageCount { get; init; }

    public uint TotalCount { get; init; }

    public T? Data { get; init; }

    public static PaginatedDomainResult<T> CreateSuccess(
        string? message,
        int statusCode,
        T data,
        uint pageNumber,
        uint pageSize,
        uint totalCount) =>
        new(
            message,
            null,
            data,
            statusCode,
            pageNumber,
            pageSize,
            totalCount);

    public static PaginatedDomainResult<T> CreateFailure(string message, int statusCode, Error error) =>
        new(message, error, null, statusCode, 0, 0, 0);
}