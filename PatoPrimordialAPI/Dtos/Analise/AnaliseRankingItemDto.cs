using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Dtos.Analise;

public record AnaliseRankingItemDto
{
    public long PatoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Pais { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public double Prioridade { get; init; }
    public string ClassePrioridade { get; init; } = string.Empty;
    public int RiscoTotal { get; init; }
    public string ClasseRisco { get; init; } = string.Empty;
    public int ValorCientifico { get; init; }
    public double CustoTransporte { get; init; }
    public int PoderioNecessario { get; init; }
    public double DistKm { get; init; }
    public DateTime CalculadoEm { get; init; }
    public IReadOnlyCollection<string> PoderTags { get; init; } = Array.Empty<string>();
}
