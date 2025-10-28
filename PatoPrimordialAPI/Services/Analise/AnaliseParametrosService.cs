using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PatoPrimordialAPI.Domain.Entities;
using PatoPrimordialAPI.Dtos.Analise;
using PatoPrimordialAPI.Infrastructure.Data;

namespace PatoPrimordialAPI.Services.Analise;

public class AnaliseParametrosService : IAnaliseParametrosService
{
    private readonly ApplicationDbContext _dbContext;

    public AnaliseParametrosService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ParametroAnaliseDto>> ListarAsync()
    {
        var parametros = await _dbContext.ParametrosAnalise
            .AsNoTracking()
            .OrderBy(p => p.Chave)
            .ToListAsync();

        return parametros
            .Select(p => new ParametroAnaliseDto
            {
                Id = p.Id,
                Chave = p.Chave,
                ValorNum = p.ValorNum,
                ValorTxt = p.ValorTxt,
                AtualizadoEm = p.AtualizadoEm
            })
            .ToList();
    }

    public async Task<AnaliseParametros> ObterConfiguracaoAsync()
    {
        var parametros = await _dbContext.ParametrosAnalise.AsNoTracking().ToListAsync();
        return AnaliseParametros.FromEntities(parametros);
    }

    public async Task AtualizarAsync(IReadOnlyCollection<AtualizarParametroAnaliseDto> parametros)
    {
        if (parametros == null || parametros.Count == 0)
        {
            return;
        }

        var chaves = parametros
            .Select(p => NormalizarChave(p.Chave))
            .Where(chave => !string.IsNullOrWhiteSpace(chave))
            .ToArray();

        var existentes = await _dbContext.ParametrosAnalise
            .Where(p => chaves.Contains(p.Chave))
            .ToDictionaryAsync(p => p.Chave);

        var agora = DateTime.UtcNow;

        foreach (var parametro in parametros)
        {
            var chaveNormalizada = NormalizarChave(parametro.Chave);
            if (string.IsNullOrWhiteSpace(chaveNormalizada))
            {
                continue;
            }

            if (!existentes.TryGetValue(chaveNormalizada, out var entidade))
            {
                entidade = new ParametroAnalise
                {
                    Chave = chaveNormalizada,
                };
                _dbContext.ParametrosAnalise.Add(entidade);
                existentes[chaveNormalizada] = entidade;
            }

            entidade.ValorNum = parametro.ValorNum;
            entidade.ValorTxt = parametro.ValorTxt;
            entidade.AtualizadoEm = agora;
        }

        await _dbContext.SaveChangesAsync();
    }

    private static string NormalizarChave(string chave)
    {
        return chave?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
