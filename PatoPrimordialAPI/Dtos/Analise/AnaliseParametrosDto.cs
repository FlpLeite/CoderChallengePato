namespace PatoPrimordialAPI.Dtos.Analise;

public record AnaliseParametrosDto
{
    public double CustoBase { get; init; }
    public double CustoPorTonelada { get; init; }
    public double CustoPorKm { get; init; }
    public double CustoPorMetro { get; init; }
    public double PesoValorCientifico { get; init; }
    public double PesoCusto { get; init; }
    public double PesoRisco { get; init; }
    public double DsinLat { get; init; }
    public double DsinLon { get; init; }
}
