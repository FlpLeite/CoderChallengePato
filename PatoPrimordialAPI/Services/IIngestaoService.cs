using System.Threading.Tasks;
using PatoPrimordialAPI.Dtos;
using PatoPrimordialAPI.Services.Ingestao;

namespace PatoPrimordialAPI.Services;

public interface IIngestaoService
{
    Task<PatoDto> RegistrarAvistamentoAsync(DroneAvistamentoRequest request);
}
