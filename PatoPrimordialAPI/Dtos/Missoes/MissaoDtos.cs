using System.Text.Json;
using PatoPrimordialAPI.Domain.Missoes;

namespace PatoPrimordialAPI.Dtos.Missoes;

public record CriarMissaoRequest(long PatoId);

public record MissaoListItemDto
{
    public long Id { get; init; }
    public long PatoId { get; init; }
    public string Status { get; init; } = string.Empty;
    public double PoderioAlocado { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime? IniciadoEm { get; init; }
    public DateTime? FinalizadoEm { get; init; }
    public string? Resultado { get; init; }
}

public record MissaoDto
{
    public long Id { get; init; }
    public long PatoId { get; init; }
    public string Status { get; init; } = string.Empty;
    public double PoderioAlocado { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime? IniciadoEm { get; init; }
    public DateTime? FinalizadoEm { get; init; }
    public TelemetriaDto Telemetria { get; init; } = new();
    public string FaseAtual { get; init; } = Fase.Idle.ToString();
    public string? ProximaFase { get; init; }
    public PlanoEstrategiaDto Estrategia { get; init; } = new();
    public PlanoDefesaDto Defesas { get; init; } = new();
    public IReadOnlyCollection<FraquezaAplicadaDto> Fraquezas { get; init; } = Array.Empty<FraquezaAplicadaDto>();
    public string? Resultado { get; init; }
}

public record TelemetriaDto
{
    public double Bateria { get; init; }
    public double Combustivel { get; init; }
    public double Integridade { get; init; }
    public double DistanciaM { get; init; }
}

public record PlanoEstrategiaDto
{
    public string Porte { get; init; } = string.Empty;
    public string ClasseRisco { get; init; } = string.Empty;
    public IReadOnlyCollection<TaticaPlanoDto> Taticas { get; init; } = Array.Empty<TaticaPlanoDto>();
}

public record TaticaPlanoDto
{
    public string Nome { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public int Prioridade { get; init; }
    public double BonusFraqueza { get; init; }
    public bool ExplorandoFraqueza { get; init; }
}

public record PlanoDefesaDto
{
    public DefesaSelecionadaDto? Primaria { get; init; }
    public DefesaSelecionadaDto? Fallback { get; init; }
}

public record DefesaSelecionadaDto
{
    public string Nome { get; init; } = string.Empty;
    public string Contramedida { get; init; } = string.Empty;
    public IReadOnlyCollection<string> TagsAmeaca { get; init; } = Array.Empty<string>();
    public int Rareza { get; init; }
    public double Mitigacao { get; init; }
}

public record FraquezaAplicadaDto
{
    public long Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public double BonusSucesso { get; init; }
}

public record MissaoTickDto
{
    public long Id { get; init; }
    public int Tick { get; init; }
    public string Fase { get; init; } = string.Empty;
    public string Evento { get; init; } = string.Empty;
    public JsonElement? Detalhe { get; init; }
    public double BateriaPct { get; init; }
    public double CombustivelPct { get; init; }
    public double IntegridadePct { get; init; }
    public double DistanciaM { get; init; }
    public bool Sucesso { get; init; }
}

public record MissaoTimelineDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyCollection<MissaoTickDto> Items { get; init; } = Array.Empty<MissaoTickDto>();
}
