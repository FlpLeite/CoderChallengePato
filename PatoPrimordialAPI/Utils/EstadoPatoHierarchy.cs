using System;
using System.Collections.Generic;

namespace PatoPrimordialAPI.Utils;

public static class EstadoPatoHierarchy
{
    private static readonly Dictionary<string, int> Hierarquia = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hibernacao"] = 1,
        ["transe"] = 2,
        ["desperto"] = 3
    };

    public static bool DeveAtualizar(string estadoAtual, string novoEstado)
    {
        var atualScore = Score(estadoAtual);
        var novoScore = Score(novoEstado);
        return novoScore >= atualScore;
    }

    public static bool RequerBpm(string estado) =>
        string.Equals(estado, "transe", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(estado, "hibernacao", StringComparison.OrdinalIgnoreCase);

    private static int Score(string estado) => Hierarquia.TryGetValue(estado ?? string.Empty, out var score) ? score : 0;
}
