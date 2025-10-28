using System.Collections.Generic;
using System.Linq;
using PatoPrimordialAPI.Domain.Entities;

namespace PatoPrimordialAPI.Services.Analise;

public record AnaliseParametros(
    double CustoBase,
    double CustoPorTonelada,
    double CustoPorKm,
    double CustoPorMetro,
    double PesoValorCientifico,
    double PesoCusto,
    double PesoRisco,
    double DsinLat,
    double DsinLon)
{
    public static AnaliseParametros Padrao { get; } = new(
        CustoBase: 2000d,
        CustoPorTonelada: 1200d,
        CustoPorKm: 5d,
        CustoPorMetro: 50d,
        PesoValorCientifico: 0.45d,
        PesoCusto: -0.35d,
        PesoRisco: -0.20d,
        DsinLat: -15.793889d,
        DsinLon: -47.882778d);

    public static AnaliseParametros FromEntities(IEnumerable<ParametroAnalise> parametros)
    {
        var config = Padrao;
        foreach (var parametro in parametros ?? Enumerable.Empty<ParametroAnalise>())
        {
            if (string.IsNullOrWhiteSpace(parametro.Chave))
            {
                continue;
            }

            var valor = parametro.ValorNum ?? (double?)null;
            config = parametro.Chave switch
            {
                AnaliseParametroChaves.CustoBase when valor.HasValue => config with { CustoBase = valor.Value },
                AnaliseParametroChaves.CustoPorTonelada when valor.HasValue => config with { CustoPorTonelada = valor.Value },
                AnaliseParametroChaves.CustoPorKm when valor.HasValue => config with { CustoPorKm = valor.Value },
                AnaliseParametroChaves.CustoPorMetro when valor.HasValue => config with { CustoPorMetro = valor.Value },
                AnaliseParametroChaves.PesoValorCientifico when valor.HasValue => config with { PesoValorCientifico = valor.Value },
                AnaliseParametroChaves.PesoCusto when valor.HasValue => config with { PesoCusto = valor.Value },
                AnaliseParametroChaves.PesoRisco when valor.HasValue => config with { PesoRisco = valor.Value },
                AnaliseParametroChaves.DsinLat when valor.HasValue => config with { DsinLat = valor.Value },
                AnaliseParametroChaves.DsinLon when valor.HasValue => config with { DsinLon = valor.Value },
                _ => config
            };
        }

        return config;
    }
}
