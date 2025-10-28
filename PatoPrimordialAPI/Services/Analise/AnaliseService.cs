using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Infrastructure.Data;
using PatoPrimordialAPI.Utils;

namespace PatoPrimordialAPI.Services.Analise;

public class AnaliseService : IAnaliseService
{
    private static readonly Dictionary<string, int> RiscoEstado = new(StringComparer.OrdinalIgnoreCase)
    {
        ["desperto"] = 80,
        ["transe"] = 50,
        ["hibernacao"] = 20
    };

    private static readonly Dictionary<string, int> RiscoPorTag = new(StringComparer.OrdinalIgnoreCase)
    {
        ["belico"] = 30,
        ["controle_mental"] = 25,
        ["elemental"] = 20,
        ["desconhecido"] = 10
    };

    private static readonly Dictionary<string, int> RarezaPorTag = new(StringComparer.OrdinalIgnoreCase)
    {
        ["belico"] = 26,
        ["controle_mental"] = 30,
        ["elemental"] = 22,
        ["desconhecido"] = 18,
        ["temporal"] = 28,
        ["ancestral"] = 30,
        ["etereo"] = 24,
        ["sonico"] = 20
    };

    private readonly ApplicationDbContext _dbContext;

    public AnaliseService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Scores CalcularParaPato(Pato pato, Coordenada dsin)
    {
        var parametros = CarregarParametros();
        return CalcularInterno(pato, dsin, parametros).Scores;
    }

    public AnaliseDetalhe CalcularDetalhado(Pato pato, Coordenada dsin)
    {
        var parametros = CarregarParametros();
        return CalcularInterno(pato, dsin, parametros);
    }

    public async Task<int> RecalcularTodosAsync(Coordenada dsin)
    {
        var parametros = CarregarParametros();
        var patos = await _dbContext.Patos.AsNoTracking().ToListAsync();
        var existentes = await _dbContext.AnalisesPatos.ToDictionaryAsync(a => a.PatoId);
        var idsAtuais = new HashSet<long>(patos.Select(p => p.Id));

        var agora = DateTime.UtcNow;

        foreach (var pato in patos)
        {
            var calculo = CalcularInterno(pato, dsin, parametros);
            if (existentes.TryGetValue(pato.Id, out var analise))
            {
                analise.CustoTransporte = calculo.Scores.CustoTransporte;
                analise.RiscoTotal = calculo.Scores.RiscoTotal;
                analise.ValorCientifico = calculo.Scores.ValorCientifico;
                analise.PoderioNecessario = calculo.Scores.PoderioNecessario;
                analise.Prioridade = calculo.Scores.Prioridade;
                analise.ClassePrioridade = calculo.Scores.ClassePrioridade;
                analise.ClasseRisco = calculo.Scores.ClasseRisco;
                analise.DistKm = calculo.Scores.DistKm;
                analise.CalculadoEm = agora;
            }
            else
            {
                _dbContext.AnalisesPatos.Add(new AnalisePato
                {
                    PatoId = pato.Id,
                    CustoTransporte = calculo.Scores.CustoTransporte,
                    RiscoTotal = calculo.Scores.RiscoTotal,
                    ValorCientifico = calculo.Scores.ValorCientifico,
                    PoderioNecessario = calculo.Scores.PoderioNecessario,
                    Prioridade = calculo.Scores.Prioridade,
                    ClassePrioridade = calculo.Scores.ClassePrioridade,
                    ClasseRisco = calculo.Scores.ClasseRisco,
                    DistKm = calculo.Scores.DistKm,
                    CalculadoEm = agora
                });
            }
        }

        foreach (var analise in existentes.Values)
        {
            if (!idsAtuais.Contains(analise.PatoId))
            {
                _dbContext.AnalisesPatos.Remove(analise);
            }
        }

        return await _dbContext.SaveChangesAsync();
    }

    private AnaliseParametros CarregarParametros()
    {
        var parametros = _dbContext.ParametrosAnalise.AsNoTracking().ToList();
        return AnaliseParametros.FromEntities(parametros);
    }

    private static AnaliseDetalhe CalcularInterno(Pato pato, Coordenada dsin, AnaliseParametros parametros)
    {
        var massaToneladas = Math.Max(0d, Convert.ToDouble(pato.PesoG) / 1_000_000d);
        var tamanhoMetros = Math.Max(0d, Convert.ToDouble(pato.AlturaCm) / 100d);
        var distKm = GeographyUtils.CalcularDistanciaMetros(dsin.Lat, dsin.Lon, pato.Latitude, pato.Longitude) / 1000d;

        var custoBase = parametros.CustoBase;
        var custoPorMassa = parametros.CustoPorTonelada * massaToneladas;
        var custoPorDistancia = parametros.CustoPorKm * distKm;
        var custoPorTamanho = parametros.CustoPorMetro * tamanhoMetros;
        var custoTotal = custoBase + custoPorMassa + custoPorDistancia + custoPorTamanho;

        var estado = NormalizarTexto(pato.Estado);
        var riscoEstado = RiscoEstado.TryGetValue(estado, out var re) ? re : 30;

        var tags = ExtrairTags(pato);
        var riscoPoder = tags
            .Select(tag => RiscoPorTag.TryGetValue(tag, out var valor) ? valor : 0)
            .DefaultIfEmpty(0)
            .Max();

        var rarezaPoder = CalcularRareza(tags);

        var riscoBpm = 0;
        if (string.Equals(estado, "transe", StringComparison.OrdinalIgnoreCase) && pato.Bpm.HasValue)
        {
            if (pato.Bpm.Value >= 110)
            {
                riscoBpm = 25;
            }
            else if (pato.Bpm.Value >= 90)
            {
                riscoBpm = 15;
            }
        }

        var mutacoes = Math.Max(0, pato.MutacoesQtde ?? 0);
        var riscoMutacoes = Math.Min(40, 3 * mutacoes);

        var riscoTotal = Math.Clamp(riscoEstado + riscoPoder + riscoBpm + riscoMutacoes, 0, 100);

        var valorBruto = 30 + 5 * mutacoes + rarezaPoder;
        var valorCientifico = Math.Clamp(valorBruto, 0, 100);
        var poderioNecessario = Math.Clamp((int)Math.Round(0.8 * riscoTotal + 10 * tamanhoMetros), 0, 100);

        var prioridade = parametros.PesoValorCientifico * valorCientifico +
                         parametros.PesoCusto * (custoTotal / 1000d) +
                         parametros.PesoRisco * riscoTotal;

        var classePrioridade = prioridade >= 20 ? "A" : prioridade >= 0 ? "B" : "C";
        var classeRisco = riscoTotal >= 70 ? "alto" : riscoTotal >= 40 ? "medio" : "baixo";

        var scores = new Scores(
            Math.Round(custoTotal, 2),
            riscoTotal,
            valorCientifico,
            poderioNecessario,
            Math.Round(distKm, 3),
            Math.Round(prioridade, 2),
            classePrioridade,
            classeRisco);

        return new AnaliseDetalhe(
            scores,
            Math.Round(massaToneladas, 4),
            Math.Round(tamanhoMetros, 3),
            Math.Round(custoBase, 2),
            Math.Round(custoPorMassa, 2),
            Math.Round(custoPorDistancia, 2),
            Math.Round(custoPorTamanho, 2),
            riscoEstado,
            riscoPoder,
            riscoBpm,
            riscoMutacoes,
            rarezaPoder,
            valorBruto);
    }

    private static IReadOnlyList<string> ExtrairTags(Pato pato)
    {
        if (string.IsNullOrWhiteSpace(pato.PoderTagsCsv))
        {
            return Array.Empty<string>();
        }

        return pato.PoderTagsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizarTexto)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToArray();
    }

    private static string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var normalized = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var filtered = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(filtered).Replace(' ', '_');
    }

    private static int CalcularRareza(IEnumerable<string> tags)
    {
        var rarezas = tags
            .Select(tag => RarezaPorTag.TryGetValue(tag, out var valor) ? valor : 12)
            .DefaultIfEmpty(0)
            .Max();

        return Math.Clamp(rarezas, 0, 30);
    }
}
