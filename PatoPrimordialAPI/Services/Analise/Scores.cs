namespace PatoPrimordialAPI.Services.Analise;

public record Scores(
    double CustoTransporte,
    int RiscoTotal,
    int ValorCientifico,
    int PoderioNecessario,
    double DistKm,
    double Prioridade,
    string ClassePrioridade,
    string ClasseRisco);
