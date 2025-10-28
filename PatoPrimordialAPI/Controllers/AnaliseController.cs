using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Dtos.Analise;
using PatoPrimordialAPI.Infrastructure.Data;
using PatoPrimordialAPI.Services.Analise;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/analise")]
public class AnaliseController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAnaliseService _analiseService;
    private readonly IAnaliseParametrosService _parametrosService;

    public AnaliseController(
        ApplicationDbContext dbContext,
        IAnaliseService analiseService,
        IAnaliseParametrosService parametrosService)
    {
        _dbContext = dbContext;
        _analiseService = analiseService;
        _parametrosService = parametrosService;
    }

    [HttpPost("recalcular")]
    public async Task<ActionResult> Recalcular()
    {
        var parametros = await _parametrosService.ObterConfiguracaoAsync();
        var dsin = new Coordenada(parametros.DsinLat, parametros.DsinLon);
        var atualizados = await _analiseService.RecalcularTodosAsync(dsin);
        return Ok(new { atualizados });
    }

    [HttpGet("patos")]
    public async Task<ActionResult<AnaliseRankingResultDto>> ListarPatos([FromQuery] AnalisePatosQueryParameters query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize is <= 0 or > 200 ? 20 : query.PageSize;

        var baseQuery = _dbContext.AnalisesPatos
            .AsNoTracking()
            .Include(a => a.Pato)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            var estado = query.Estado.Trim().ToLowerInvariant();
            if (estado == "com-referencia")
            {
                baseQuery = baseQuery.Where(a => a.Pato.PontoReferenciaId != null);
            }
            else
            {
                baseQuery = baseQuery.Where(a => a.Pato.Estado == estado);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Pais))
        {
            var pais = query.Pais.Trim();
            baseQuery = baseQuery.Where(a => a.Pato.Pais == pais);
        }

        if (!string.IsNullOrWhiteSpace(query.Risco))
        {
            var risco = query.Risco.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(a => a.ClasseRisco == risco);
        }

        var ordenado = query.Ordem?.ToLowerInvariant() switch
        {
            "risco" => baseQuery.OrderByDescending(a => a.RiscoTotal),
            "valor" => baseQuery.OrderByDescending(a => a.ValorCientifico),
            "custo" => baseQuery.OrderBy(a => a.CustoTransporte),
            "poderio" => baseQuery.OrderByDescending(a => a.PoderioNecessario),
            "dist" => baseQuery.OrderBy(a => a.DistKm),
            _ => baseQuery.OrderByDescending(a => a.Prioridade)
        };

        var total = await baseQuery.CountAsync();

        var itens = await ordenado
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AnaliseRankingItemDto
            {
                PatoId = a.PatoId,
                Codigo = a.Pato.Codigo,
                Estado = a.Pato.Estado,
                Pais = a.Pato.Pais,
                Cidade = a.Pato.Cidade,
                Prioridade = a.Prioridade,
                ClassePrioridade = a.ClassePrioridade,
                RiscoTotal = a.RiscoTotal,
                ClasseRisco = a.ClasseRisco,
                ValorCientifico = a.ValorCientifico,
                CustoTransporte = a.CustoTransporte,
                PoderioNecessario = a.PoderioNecessario,
                DistKm = a.DistKm,
                CalculadoEm = a.CalculadoEm,
                PoderTags = string.IsNullOrWhiteSpace(a.Pato.PoderTagsCsv)
                    ? Array.Empty<string>()
                    : a.Pato.PoderTagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            })
            .ToListAsync();

        var resumoEstado = await baseQuery
            .GroupBy(a => a.Pato.Estado)
            .Select(g => new { Estado = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        var resumoRisco = await baseQuery
            .GroupBy(a => a.ClasseRisco)
            .Select(g => new { Classe = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        return Ok(new AnaliseRankingResultDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = itens,
            ResumoPorEstado = resumoEstado.ToDictionary(x => x.Estado ?? string.Empty, x => x.Quantidade),
            ResumoPorRisco = resumoRisco.ToDictionary(x => x.Classe ?? string.Empty, x => x.Quantidade)
        });
    }

    [HttpGet("patos/{patoId:long}")]
    public async Task<ActionResult<AnaliseDetalheDto>> Detalhar(long patoId)
    {
        var registro = await _dbContext.AnalisesPatos
            .Include(a => a.Pato)
            .FirstOrDefaultAsync(a => a.PatoId == patoId);

        if (registro == null)
        {
            return NotFound();
        }

        var parametros = await _parametrosService.ObterConfiguracaoAsync();
        var dsin = new Coordenada(parametros.DsinLat, parametros.DsinLon);
        var detalhe = _analiseService.CalcularDetalhado(registro.Pato, dsin);

        var tags = string.IsNullOrWhiteSpace(registro.Pato.PoderTagsCsv)
            ? Array.Empty<string>()
            : registro.Pato.PoderTagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var dto = new AnaliseDetalheDto
        {
            PatoId = registro.PatoId,
            Codigo = registro.Pato.Codigo,
            PoderNome = registro.Pato.PoderNome,
            PoderTags = tags,
            Scores = new AnaliseScoresDto
            {
                CustoTransporte = detalhe.Scores.CustoTransporte,
                RiscoTotal = detalhe.Scores.RiscoTotal,
                ValorCientifico = detalhe.Scores.ValorCientifico,
                PoderioNecessario = detalhe.Scores.PoderioNecessario,
                DistKm = detalhe.Scores.DistKm,
                Prioridade = detalhe.Scores.Prioridade,
                ClassePrioridade = detalhe.Scores.ClassePrioridade,
                ClasseRisco = detalhe.Scores.ClasseRisco
            },
            Entradas = new AnaliseDetalheEntradasDto
            {
                MassaToneladas = detalhe.MassaToneladas,
                TamanhoMetros = detalhe.TamanhoMetros,
                DistanciaKm = detalhe.Scores.DistKm,
                Mutacoes = registro.Pato.MutacoesQtde ?? 0,
                Bpm = registro.Pato.Bpm,
                Estado = registro.Pato.Estado
            },
            Componentes = new AnaliseDetalheComponentesDto
            {
                CustoBase = detalhe.CustoBase,
                CustoPorMassa = detalhe.CustoPorMassa,
                CustoPorDistancia = detalhe.CustoPorDistancia,
                CustoPorTamanho = detalhe.CustoPorTamanho,
                RiscoEstado = detalhe.RiscoEstado,
                RiscoPoder = detalhe.RiscoPoder,
                RiscoBpm = detalhe.RiscoBpm,
                RiscoMutacoes = detalhe.RiscoMutacoes,
                RarezaPoder = detalhe.RarezaPoder,
                ValorCientificoBase = detalhe.ValorCientificoBruto
            },
            Parametros = new AnaliseParametrosDto
            {
                CustoBase = parametros.CustoBase,
                CustoPorTonelada = parametros.CustoPorTonelada,
                CustoPorKm = parametros.CustoPorKm,
                CustoPorMetro = parametros.CustoPorMetro,
                PesoValorCientifico = parametros.PesoValorCientifico,
                PesoCusto = parametros.PesoCusto,
                PesoRisco = parametros.PesoRisco,
                DsinLat = parametros.DsinLat,
                DsinLon = parametros.DsinLon
            },
            CalculadoEm = registro.CalculadoEm
        };

        return Ok(dto);
    }

    [HttpGet("parametros")]
    public async Task<ActionResult<IReadOnlyCollection<ParametroAnaliseDto>>> ListarParametros()
    {
        var parametros = await _parametrosService.ListarAsync();
        return Ok(parametros);
    }

    [HttpPut("parametros")]
    public async Task<ActionResult> AtualizarParametros([FromBody] IReadOnlyCollection<AtualizarParametroAnaliseDto> parametros)
    {
        await _parametrosService.AtualizarAsync(parametros);
        return NoContent();
    }
}

public record AnalisePatosQueryParameters
{
    [FromQuery(Name = "ordem")]
    public string? Ordem { get; init; }

    [FromQuery(Name = "estado")]
    public string? Estado { get; init; }

    [FromQuery(Name = "risco")]
    public string? Risco { get; init; }

    [FromQuery(Name = "pais")]
    public string? Pais { get; init; }

    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;
}
