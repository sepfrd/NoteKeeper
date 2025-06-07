namespace NoteKeeper.Domain.Common;

public record PaginatedDomainResult<T> : DomainResult where T : class, IEnumerable<object>
{
    protected PaginatedDomainResult(string? message, int statusCode, T? data, uint pageNumber, uint pageSize, uint totalCount)
        : base(message, statusCode)
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
        int resultCode,
        T data,
        uint pageNumber,
        uint pageSize,
        uint totalCount) =>
        new(message, resultCode, data, pageNumber, pageSize, totalCount);

    public static PaginatedDomainResult<T> CreateFailure(string message, int resultCode) =>
        new(message, resultCode, null, 0, 0, 0);
}