using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Dtos;
using PatoPrimordialAPI.Infrastructure.Data;
using PatoPrimordialAPI.Utils;

namespace PatoPrimordialAPI.Services.Ingestao;

public class IngestaoService : IIngestaoService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PatoObservationProcessor _processor;

    public IngestaoService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _processor = new PatoObservationProcessor();
    }

    public async Task<PatoDto> RegistrarAvistamentoAsync(DroneAvistamentoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NumeroSerie))
        {
            throw new BadHttpRequestException("numero_serie é obrigatório");
        }

        var estadoNormalizado = NormalizarEstado(request.EstadoPato);
        if (estadoNormalizado is not ("desperto" or "transe" or "hibernacao"))
        {
            throw new BadHttpRequestException("estado_pato deve ser desperto, transe ou hibernacao");
        }

        if (EstadoPatoHierarchy.RequerBpm(estadoNormalizado) && !request.Bpm.HasValue)
        {
            throw new BadHttpRequestException("bpm é obrigatório para estado transe ou hibernacao");
        }

        var alturaCm = MeasurementConverter.ToCentimeters(request.AlturaValor, request.AlturaUnidade);
        var pesoG = MeasurementConverter.ToGrams(request.PesoValor, request.PesoUnidade);
        var precisaoM = MeasurementConverter.ToMeters(request.PrecisaoValor, request.PrecisaoUnidade);
        var precisaoLimitadaM = MeasurementValidator.LimitarPrecisao(precisaoM);
        var precisaoValorNormalizado = MeasurementConverter.FromMeters(precisaoLimitadaM, request.PrecisaoUnidade);

        MeasurementValidator.ValidarAltura(alturaCm);
        MeasurementValidator.ValidarPeso(pesoG);
        MeasurementValidator.ValidarPrecisao(precisaoLimitadaM);

        var confianca = 1d / (double)precisaoLimitadaM;
        var poderTagsCsv = request.Poder?.Tags is { Count: > 0 } tags
            ? string.Join(',', tags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()))
            : null;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var fabricante = await ObterOuCriarFabricanteAsync(request);
        var drone = await ObterOuCriarDroneAsync(request, fabricante);

        var codigoPato = PatoCodigoGenerator.GerarCodigo(request.Pais, request.Cidade, request.Latitude, request.Longitude);
        var pato = await _dbContext.Patos
            .Include(p => p.PontoReferencia)
            .FirstOrDefaultAsync(p => p.Codigo == codigoPato);

        var pontosReferencia = await _dbContext.PontosReferencia.ToListAsync();
        var pontoRelacionado = pontosReferencia
            .Select(p => new
            {
                Ponto = p,
                Distancia = GeographyUtils.CalcularDistanciaMetros(request.Latitude, request.Longitude, p.Latitude, p.Longitude)
            })
            .OrderBy(x => x.Distancia)
            .FirstOrDefault(x => x.Distancia <= x.Ponto.RaioMetros)?.Ponto;

        var agora = DateTime.UtcNow;
        var observacao = new ObservacaoNormalizada(
            alturaCm,
            pesoG,
            precisaoLimitadaM,
            confianca,
            request.Cidade.Trim(),
            request.Pais.Trim(),
            request.Latitude,
            request.Longitude,
            estadoNormalizado,
            request.Bpm,
            request.MutacoesQtde,
            request.Poder?.Nome,
            request.Poder?.Descricao,
            poderTagsCsv,
            agora,
            pontoRelacionado);

        if (pato is null)
        {
            pato = _processor.CriarNovo(codigoPato, observacao);
            await _dbContext.Patos.AddAsync(pato);
        }
        else
        {
            _processor.Aplicar(pato, observacao);
            _dbContext.Patos.Update(pato);
        }

        var avistamento = new Avistamento
        {
            DroneId = drone.Id,
            Drone = drone,
            PatoId = pato.Id,
            Pato = pato,
            AlturaValor = request.AlturaValor,
            AlturaUnidade = request.AlturaUnidade,
            PesoValor = request.PesoValor,
            PesoUnidade = request.PesoUnidade,
            AlturaCm = alturaCm,
            PesoG = pesoG,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PrecisaoValor = precisaoValorNormalizado,
            PrecisaoUnidade = request.PrecisaoUnidade,
            PrecisaoM = precisaoLimitadaM,
            Confianca = confianca,
            Cidade = request.Cidade.Trim(),
            Pais = request.Pais.Trim(),
            EstadoPato = estadoNormalizado,
            Bpm = request.Bpm,
            MutacoesQtde = request.MutacoesQtde,
            PoderNome = request.Poder?.Nome,
            PoderDescricao = request.Poder?.Descricao,
            PoderTagsCsv = poderTagsCsv,
            CriadoEm = agora
        };

        await _dbContext.Avistamentos.AddAsync(avistamento);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

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
            PontoReferenciaId = pato.PontoReferenciaId,
            PontoReferenciaNome = pato.PontoReferencia?.Nome,
            CriadoEm = pato.CriadoEm,
            AtualizadoEm = pato.AtualizadoEm
        };
    }

    private async Task<Fabricante?> ObterOuCriarFabricanteAsync(DroneAvistamentoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FabricanteNome))
        {
            return null;
        }

        var nome = request.FabricanteNome.Trim();
        var fabricante = await _dbContext.Fabricantes.FirstOrDefaultAsync(f => f.Nome == nome);
        if (fabricante is not null)
        {
            var atualizado = false;
            if (!string.IsNullOrWhiteSpace(request.Marca) && !string.Equals(fabricante.Marca, request.Marca.Trim(), StringComparison.Ordinal))
            {
                fabricante.Marca = request.Marca.Trim();
                atualizado = true;
            }

            if (!string.IsNullOrWhiteSpace(request.PaisOrigem) && !string.Equals(fabricante.PaisOrigem, request.PaisOrigem.Trim(), StringComparison.Ordinal))
            {
                fabricante.PaisOrigem = request.PaisOrigem.Trim();
                atualizado = true;
            }

            if (atualizado)
            {
                _dbContext.Fabricantes.Update(fabricante);
            }

            return fabricante;
        }

        fabricante = new Fabricante
        {
            Nome = nome,
            Marca = request.Marca?.Trim(),
            PaisOrigem = request.PaisOrigem?.Trim()
        };

        await _dbContext.Fabricantes.AddAsync(fabricante);
        await _dbContext.SaveChangesAsync();
        return fabricante;
    }

    private async Task<Drone> ObterOuCriarDroneAsync(DroneAvistamentoRequest request, Fabricante? fabricante)
    {
        Drone? drone = null;
        if (request.DroneId > 0)
        {
            drone = await _dbContext.Drones.FirstOrDefaultAsync(d => d.Id == request.DroneId);
        }

        if (drone is null)
        {
            var numeroSerie = request.NumeroSerie.Trim();
            drone = await _dbContext.Drones.FirstOrDefaultAsync(d => d.NumeroSerie == numeroSerie);
        }

        if (drone is null)
        {
            drone = new Drone
            {
                NumeroSerie = request.NumeroSerie.Trim(),
                FabricanteId = fabricante?.Id,
                Status = "operacional",
                UltimoContatoEm = DateTime.UtcNow
            };

            await _dbContext.Drones.AddAsync(drone);
        }
        else
        {
            drone.NumeroSerie = request.NumeroSerie.Trim();
            drone.FabricanteId = fabricante?.Id;
            drone.UltimoContatoEm = DateTime.UtcNow;
            _dbContext.Drones.Update(drone);
        }

        await _dbContext.SaveChangesAsync();
        return drone;
    }

    private static string NormalizarEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            return string.Empty;
        }

        var normalized = estado.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
