namespace PatoPrimordialAPI.Dtos.Analise;

public record AnaliseDetalheEntradasDto
{
    public double MassaToneladas { get; init; }
    public double TamanhoMetros { get; init; }
    public double DistanciaKm { get; init; }
    public int Mutacoes { get; init; }
    public int? Bpm { get; init; }
    public string Estado { get; init; } = string.Empty;
}
