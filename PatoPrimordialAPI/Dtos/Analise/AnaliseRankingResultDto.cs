using System.Collections.Generic;

namespace PatoPrimordialAPI.Dtos.Analise;

public record AnaliseRankingResultDto
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required IReadOnlyCollection<AnaliseRankingItemDto> Items { get; init; } = new List<AnaliseRankingItemDto>();
    public IReadOnlyDictionary<string, int>? ResumoPorEstado { get; init; }
    public IReadOnlyDictionary<string, int>? ResumoPorRisco { get; init; }
}
