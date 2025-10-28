using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Dtos;
using PatoPrimordialAPI.Infrastructure.Data;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/patos")]
public class PatosController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public PatosController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PatoListItemDto>>> Listar([FromQuery] PatosQueryParameters query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize is <= 0 or > 200 ? 20 : query.PageSize;

        var patosQuery = _dbContext.Patos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            var estado = query.Estado.Trim().ToLower();
            patosQuery = patosQuery.Where(p => p.Estado != null && p.Estado.ToLower() == estado);
        }

        if (!string.IsNullOrWhiteSpace(query.Pais))
        {
            var pais = query.Pais.Trim();
            patosQuery = patosQuery.Where(p => p.Pais == pais);
        }

        if (!string.IsNullOrWhiteSpace(query.Cidade))
        {
            var cidade = query.Cidade.Trim();
            patosQuery = patosQuery.Where(p => p.Cidade == cidade);
        }

        if (query.MutacoesMin.HasValue)
        {
            patosQuery = patosQuery.Where(p => p.MutacoesQtde >= query.MutacoesMin.Value);
        }

        if (query.MutacoesMax.HasValue)
        {
            patosQuery = patosQuery.Where(p => p.MutacoesQtde <= query.MutacoesMax.Value);
        }

        var patosFiltrados = await patosQuery.ToListAsync();

        var total = patosFiltrados.Count;

        var items = patosFiltrados
            .OrderByDescending(p => p.AtualizadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatoListItemDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Cidade = p.Cidade,
                Pais = p.Pais,
                Estado = p.Estado,
                MutacoesQtde = p.MutacoesQtde,
                PrecisaoM = p.PrecisaoM
            })
            .ToList();

        var resumoEstado = patosFiltrados
            .GroupBy(p => p.Estado)
            .Select(g => new { Estado = g.Key, Quantidade = g.Count() })
            .ToList();

        return Ok(new PagedResult<PatoListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items,
            ResumoPorEstado = resumoEstado.ToDictionary(x => x.Estado, x => x.Quantidade)
        });
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<PatosKpiDto>> ObterKpis()
    {
        var patos = await _dbContext.Patos
            .AsNoTracking()
            .ToListAsync();

        var total = patos.Count;
        var porEstado = patos
            .GroupBy(p => p.Estado)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToList();

        return Ok(new PatosKpiDto
        {
            Total = total,
            PorEstado = porEstado.ToDictionary(x => x.Key ?? string.Empty, x => x.Count)
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PatoDto>> Obter(long id)
    {
        var pato = await _dbContext.Patos
            .Include(p => p.PontoReferencia)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pato == null)
        {
            return NotFound();
        }

        return Ok(MapearPato(pato));
    }

    [HttpGet("{id:long}/historico")]
    public async Task<ActionResult<IReadOnlyCollection<AvistamentoDto>>> Historico(long id)
    {
        var existe = await _dbContext.Patos.AnyAsync(p => p.Id == id);
        if (!existe)
        {
            return NotFound();
        }

        var avistamentos = await _dbContext.Avistamentos
            .AsNoTracking()
            .Include(a => a.Drone)
            .Where(a => a.PatoId == id)
            .OrderByDescending(a => a.CriadoEm)
            .Take(200)
            .ToListAsync();

        var avistamentosDto = avistamentos
            .Select(a => new AvistamentoDto
            {
                Id = a.Id.ToString(CultureInfo.InvariantCulture),
                DroneId = a.DroneId.ToString(CultureInfo.InvariantCulture),
                DroneNumeroSerie = a.Drone?.NumeroSerie ?? string.Empty,
                AlturaValor = a.AlturaValor,
                AlturaUnidade = a.AlturaUnidade,
                PesoValor = a.PesoValor,
                PesoUnidade = a.PesoUnidade,
                PrecisaoValor = a.PrecisaoValor,
                PrecisaoUnidade = a.PrecisaoUnidade,
                PrecisaoM = a.PrecisaoM,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                Cidade = a.Cidade,
                Pais = a.Pais,
                EstadoPato = a.EstadoPato,
                Bpm = a.Bpm,
                MutacoesQtde = a.MutacoesQtde,
                CriadoEm = a.CriadoEm
            })
            .ToList();

        return Ok(avistamentosDto);
    }

    private static PatoDto MapearPato(Pato pato)
    {
        return new PatoDto
        {
            Id = pato.Id,
            Codigo = pato.Codigo,
            AlturaCm = pato.AlturaCm,
            PesoG = pato.PesoG,
            Cidade = pato.Cidade,
            Pais = pato.Pais,
            Latitude = pato.Latitude,
            Longitude = pato.Longitude,
            PrecisaoM = pato.PrecisaoM,
            Estado = pato.Estado,
            Bpm = pato.Bpm,
            MutacoesQtde = pato.MutacoesQtde,
            PoderNome = pato.PoderNome,
            PoderDescricao = pato.PoderDescricao,
            PoderTags = string.IsNullOrWhiteSpace(pato.PoderTagsCsv)
                ? Array.Empty<string>()
                : pato.PoderTagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            PontoReferenciaId = pato.PontoReferencia?.Id,
            PontoReferenciaNome = pato.PontoReferencia?.Nome,
            CriadoEm = pato.CriadoEm,
            AtualizadoEm = pato.AtualizadoEm
        };
    }
}

public record PatosQueryParameters
{
    [FromQuery(Name = "estado")]
    public string? Estado { get; init; }

    [FromQuery(Name = "pais")]
    public string? Pais { get; init; }

    [FromQuery(Name = "cidade")]
    public string? Cidade { get; init; }

    [FromQuery(Name = "mutMin")]
    public int? MutacoesMin { get; init; }

    [FromQuery(Name = "mutMax")]
    public int? MutacoesMax { get; init; }

    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;
}
