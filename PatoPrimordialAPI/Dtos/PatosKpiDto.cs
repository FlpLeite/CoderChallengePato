using System.Collections.Generic;

namespace PatoPrimordialAPI.Dtos;

public record PatosKpiDto
{
    public int Total { get; init; }
    public IReadOnlyDictionary<string, int> PorEstado { get; init; } = new Dictionary<string, int>();
}
