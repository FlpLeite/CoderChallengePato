namespace PatoPrimordialAPI.Services.Analise;

public record AnaliseDetalhe(
    Scores Scores,
    double MassaToneladas,
    double TamanhoMetros,
    double CustoBase,
    double CustoPorMassa,
    double CustoPorDistancia,
    double CustoPorTamanho,
    int RiscoEstado,
    int RiscoPoder,
    int RiscoBpm,
    int RiscoMutacoes,
    int RarezaPoder,
    int ValorCientificoBruto);
