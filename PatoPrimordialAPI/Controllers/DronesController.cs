using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Dtos;
using PatoPrimordialAPI.Infrastructure.Data;
using PatoPrimordialAPI.Utils;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/drones")]
public class DronesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public DronesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("saude")]
    public async Task<ActionResult<DroneSaudeDto>> ObterSaude()
    {
        var drones = await _dbContext.Drones
            .AsNoTracking()
            .ToListAsync();

        var contagem = drones
            .GroupBy(d => d.Status)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToList();

        var avistamentosRecentes = await _dbContext.Avistamentos
            .AsNoTracking()
            .OrderByDescending(a => a.CriadoEm)
            .Take(200)
            .ToListAsync();

        var ultimoPorDrone = avistamentosRecentes
            .GroupBy(a => a.DroneId)
            .ToDictionary(g => g.Key, g => g.First());

        var dronesDto = drones
            .Select(d => new DroneSaudeItemDto
            {
                Id = d.Id.ToString(CultureInfo.InvariantCulture),
                NumeroSerie = d.NumeroSerie,
                Status = d.Status,
                UltimoContatoEm = d.UltimoContatoEm,
                UltimoAvistamento = ultimoPorDrone.TryGetValue(d.Id, out var avistamento)
                    ? new AvistamentoResumoDto
                    {
                        Id = avistamento.Id.ToString(CultureInfo.InvariantCulture),
                        CriadoEm = avistamento.CriadoEm,
                        EstadoPato = avistamento.EstadoPato,
                        PrecisaoM = avistamento.PrecisaoM
                    }
                    : null
            })
            .ToList();

        return Ok(new DroneSaudeDto
        {
            ContagemPorStatus = contagem.ToDictionary(x => x.Key ?? "desconhecido", x => x.Total),
            Drones = dronesDto
        });
    }

    [HttpPost]
    public async Task<ActionResult<DroneSaudeItemDto>> Criar([FromBody] CriarDroneRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NumeroSerie))
        {
            return BadRequest("Informe o número de série do drone.");
        }

        var numeroSerie = request.NumeroSerie.Trim();

        var jaExiste = await _dbContext.Drones
            .AnyAsync(d => d.NumeroSerie == numeroSerie);

        if (jaExiste)
        {
            return Conflict($"Já existe um drone cadastrado com o número de série {numeroSerie}.");
        }

        Fabricante? fabricante = null;
        if (!string.IsNullOrWhiteSpace(request.FabricanteNome))
        {
            var nome = request.FabricanteNome.Trim();
            var marca = request.Marca?.Trim();
            var paisOrigem = request.PaisOrigem?.Trim();

            fabricante = await _dbContext.Fabricantes
                .FirstOrDefaultAsync(f =>
                    f.Nome == nome &&
                    (f.Marca ?? string.Empty) == (marca ?? string.Empty) &&
                    (f.PaisOrigem ?? string.Empty) == (paisOrigem ?? string.Empty));

            if (fabricante is null)
            {
                fabricante = new Fabricante
                {
                    Nome = nome,
                    Marca = marca,
                    PaisOrigem = paisOrigem
                };

                await _dbContext.Fabricantes.AddAsync(fabricante);
                await _dbContext.SaveChangesAsync();
            }
        }

        var status = string.IsNullOrWhiteSpace(request.Status)
            ? "operacional"
            : request.Status.Trim().ToLowerInvariant();

        decimal? precisaoNominalMinCm = request.PrecisaoNominalMinCm;
        if (precisaoNominalMinCm.HasValue)
        {
            precisaoNominalMinCm = MeasurementValidator.LimitarPrecisaoNominalMinCm(precisaoNominalMinCm.Value);
        }

        decimal? precisaoNominalMaxM = request.PrecisaoNominalMaxM;
        if (precisaoNominalMaxM.HasValue)
        {
            precisaoNominalMaxM = MeasurementValidator.LimitarPrecisao(precisaoNominalMaxM.Value);
        }

        if (precisaoNominalMinCm.HasValue && precisaoNominalMaxM.HasValue)
        {
            var minimoEmMetros = precisaoNominalMinCm.Value / 100m;
            if (minimoEmMetros > precisaoNominalMaxM.Value)
            {
                precisaoNominalMinCm = precisaoNominalMaxM.Value * 100m;
            }
        }

        var drone = new Drone
        {
            NumeroSerie = numeroSerie,
            Status = status,
            FabricanteId = fabricante?.Id,
            PrecisaoNominalMinCm = precisaoNominalMinCm,
            PrecisaoNominalMaxM = precisaoNominalMaxM,
            UltimoContatoEm = DateTime.UtcNow
        };

        await _dbContext.Drones.AddAsync(drone);
        await _dbContext.SaveChangesAsync();

        var dto = new DroneSaudeItemDto
        {
            Id = drone.Id.ToString(CultureInfo.InvariantCulture),
            NumeroSerie = drone.NumeroSerie,
            Status = drone.Status,
            UltimoContatoEm = drone.UltimoContatoEm,
            UltimoAvistamento = null
        };

        return CreatedAtAction(nameof(ObterSaude), new { }, dto);
    }
}
