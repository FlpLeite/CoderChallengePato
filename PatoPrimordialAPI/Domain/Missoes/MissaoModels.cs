using System.Text.Json;
using System.Text.Json.Serialization;

namespace PatoPrimordialAPI.Domain.Missoes;

public enum Fase
{
    Idle,
    Takeoff,
    Cruise,
    Approach,
    Engage,
    Egress,
    Land
}

public enum MissaoStatus
{
    Planejada,
    EmExecucao,
    Abortada,
    Concluida
}

public record Telemetria
{
    public double Bateria { get; set; } = 100;
    public double Combustivel { get; set; } = 100;
    public double Integridade { get; set; } = 100;
    public double DistanciaM { get; set; } = 1500;
}

public class Missao
{
    public long Id { get; set; }
    public long PatoId { get; set; }
    public MissaoStatus Status { get; set; }
    public double PoderioAlocado { get; set; }
    public string EstrategiaJson { get; set; } = string.Empty;
    public string DefesaJson { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? IniciadoEm { get; set; }
    public DateTime? FinalizadoEm { get; set; }
    public double RiscoTotal { get; set; }
    public string? Resultado { get; set; }
}

public class MissaoTick
{
    public long Id { get; set; }
    public long MissaoId { get; set; }
    public int Tick { get; set; }
    public Fase Fase { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string? DetalheJson { get; set; }
    public double BateriaPct { get; set; }
    public double CombustivelPct { get; set; }
    public double IntegridadePct { get; set; }
    public double DistanciaM { get; set; }
    public bool Sucesso { get; set; }
}

public record RegraTatica
{
    public long Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public RegraTaticaCondicao Condicao { get; init; } = new();
    public RegraTaticaAcao Acao { get; init; } = new();
    public int Prioridade { get; init; }
}

public record RegraTaticaCondicao
{
    public string? Porte { get; init; }
    public string? ClasseRisco { get; init; }
    public string? Estado { get; init; }
    public string? PoderTag { get; init; }
    public double? AlturaMinCm { get; init; }
    public double? AlturaMaxCm { get; init; }
    public int? MutacoesMin { get; init; }
    public int? MutacoesMax { get; init; }
    public double? BpmMin { get; init; }
}

public record RegraTaticaAcao
{
    public string Tipo { get; init; } = "padrao";
    public string? Tatica { get; init; }
    public string? Observacao { get; init; }
}

public record RegraDefesa
{
    public long Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public IReadOnlyCollection<string> TagsAmeaca { get; init; } = Array.Empty<string>();
    public string Contramedida { get; init; } = string.Empty;
    public int Rareza { get; init; }
    public double Mitigacao { get; init; }
    public bool Fallback { get; init; }
}

public record FraquezaCatalogo
{
    public long Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public FraquezaCondicao Condicao { get; init; } = new();
    public FraquezaEfeito Efeito { get; init; } = new();
}

public record FraquezaCondicao
{
    public string? Estado { get; init; }
    public string? Porte { get; init; }
    public string? PoderTag { get; init; }
    public int? MutacoesMin { get; init; }
    public int? MutacoesMax { get; init; }
}

public record FraquezaEfeito
{
    public double BonusSucesso { get; init; }
    public string Descricao { get; init; } = string.Empty;
}

public static class MissaoJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
