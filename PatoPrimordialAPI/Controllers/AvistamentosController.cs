using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Dtos;
using PatoPrimordialAPI.Infrastructure.Data;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/avistamentos")]
public class AvistamentosController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AvistamentosController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("recentes")]
    public async Task<ActionResult<IReadOnlyCollection<AvistamentoDto>>> Recentes()
    {
        var avistamentos = await _dbContext.Avistamentos
            .AsNoTracking()
            .Include(a => a.Drone)
            .OrderByDescending(a => a.CriadoEm)
            .Take(10)
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
}
