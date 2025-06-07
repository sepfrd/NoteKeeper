using System.Net;

namespace NoteKeeper.Infrastructure.Common.Dtos;

public class ResponseDto<T>
{
    public T? Data { get; init; }

    public bool IsSuccess { get; init; }

    public string? Message { get; init; }

    public required HttpStatusCode HttpStatusCode { get; init; }
}