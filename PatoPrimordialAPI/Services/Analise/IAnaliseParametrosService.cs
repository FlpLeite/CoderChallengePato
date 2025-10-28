using System.Collections.Generic;
using System.Threading.Tasks;
using PatoPrimordialAPI.Dtos.Analise;

namespace PatoPrimordialAPI.Services.Analise;

public interface IAnaliseParametrosService
{
    Task<IReadOnlyCollection<ParametroAnaliseDto>> ListarAsync();
    Task<AnaliseParametros> ObterConfiguracaoAsync();
    Task AtualizarAsync(IReadOnlyCollection<AtualizarParametroAnaliseDto> parametros);
}
