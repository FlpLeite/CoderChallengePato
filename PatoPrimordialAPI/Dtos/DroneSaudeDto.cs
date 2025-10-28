using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Dtos;

public record DroneSaudeDto
{
    public IReadOnlyDictionary<string, int> ContagemPorStatus { get; init; } = new Dictionary<string, int>();
    public IReadOnlyCollection<DroneSaudeItemDto> Drones { get; init; } = Array.Empty<DroneSaudeItemDto>();
}

public record DroneSaudeItemDto
{
    public string Id { get; init; } = string.Empty;
    public string NumeroSerie { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? UltimoContatoEm { get; init; }
    public AvistamentoResumoDto? UltimoAvistamento { get; init; }
}

public record AvistamentoResumoDto
{
    public string Id { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; }
    public string EstadoPato { get; init; } = string.Empty;
    public decimal PrecisaoM { get; init; }
}
