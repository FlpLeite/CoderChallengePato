using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Dtos.Analise;

public record AnaliseDetalheDto
{
    public long PatoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string? PoderNome { get; init; }
    public IReadOnlyCollection<string> PoderTags { get; init; } = Array.Empty<string>();
    public AnaliseScoresDto Scores { get; init; } = new();
    public AnaliseDetalheEntradasDto Entradas { get; init; } = new();
    public AnaliseDetalheComponentesDto Componentes { get; init; } = new();
    public AnaliseParametrosDto Parametros { get; init; } = new();
    public DateTime CalculadoEm { get; init; }
}
