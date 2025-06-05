namespace NoteKeeper.DataAccess.Common;

public record PaginatedResult<T>(
    int PageNumber,
    int PageSize,
    long TotalCount,
    IEnumerable<T> Items)
    where T : class;