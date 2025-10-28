namespace PatoPrimordialAPI.Dtos.Analise;

public record AnaliseDetalheComponentesDto
{
    public double CustoBase { get; init; }
    public double CustoPorMassa { get; init; }
    public double CustoPorDistancia { get; init; }
    public double CustoPorTamanho { get; init; }
    public int RiscoEstado { get; init; }
    public int RiscoPoder { get; init; }
    public int RiscoBpm { get; init; }
    public int RiscoMutacoes { get; init; }
    public int RarezaPoder { get; init; }
    public int ValorCientificoBase { get; init; }
}
