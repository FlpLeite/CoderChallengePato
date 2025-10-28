using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Dtos;
using PatoPrimordialAPI.Infrastructure.Data;
using PatoPrimordialAPI.Utils;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/pontos-referencia")]
public class PontosReferenciaController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public PontosReferenciaController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<PontoReferenciaDto>> Criar([FromBody] CreatePontoReferenciaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var ponto = new PontoReferencia
        {
            Nome = request.Nome.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RaioMetros = request.RaioMetros
        };

        _dbContext.PontosReferencia.Add(ponto);
        await _dbContext.SaveChangesAsync();

        var dto = new PontoReferenciaDto
        {
            Id = ponto.Id,
            Nome = ponto.Nome,
            Latitude = ponto.Latitude,
            Longitude = ponto.Longitude,
            RaioMetros = ponto.RaioMetros,
            DistanciaMetros = null
        };

        return Created($"/api/pontos-referencia/{ponto.Id}", dto);
    }

    [HttpGet("near")]
    public async Task<ActionResult<IReadOnlyCollection<PontoReferenciaDto>>> Proximos([FromQuery] double lat, [FromQuery] double lon)
    {
        var pontos = await _dbContext.PontosReferencia.ToListAsync();

        var resultados = pontos
            .Select(p =>
            {
                var distancia = GeographyUtils.CalcularDistanciaMetros(lat, lon, p.Latitude, p.Longitude);
                return new PontoReferenciaDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    RaioMetros = p.RaioMetros,
                    DistanciaMetros = distancia
                };
            })
            .Where(r => r.DistanciaMetros <= r.RaioMetros)
            .OrderBy(r => r.DistanciaMetros)
            .ToList();

        return Ok(resultados);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PontoReferenciaDto>>> ListarTodos()
    {
        var pontos = await _dbContext.PontosReferencia
            .AsNoTracking()
            .Select(p => new PontoReferenciaDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                RaioMetros = p.RaioMetros,
                DistanciaMetros = null
            })
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return Ok(pontos);
    }
}
