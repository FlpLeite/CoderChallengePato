namespace PatoPrimordialAPI.Dtos.Analise;

public record AnaliseScoresDto
{
    public double CustoTransporte { get; init; }
    public int RiscoTotal { get; init; }
    public int ValorCientifico { get; init; }
    public int PoderioNecessario { get; init; }
    public double DistKm { get; init; }
    public double Prioridade { get; init; }
    public string ClassePrioridade { get; init; } = string.Empty;
    public string ClasseRisco { get; init; } = string.Empty;
}
