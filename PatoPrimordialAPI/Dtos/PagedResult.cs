using System.Collections.Generic;

namespace PatoPrimordialAPI.Dtos;

public record PagedResult<T>
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required IReadOnlyCollection<T> Items { get; init; }
    public IReadOnlyDictionary<string, int>? ResumoPorEstado { get; init; }
}
