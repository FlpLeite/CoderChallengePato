using System.Threading.Tasks;
using PatoPrimordialAPI.Domain.Entities;

namespace PatoPrimordialAPI.Services.Analise;

public record Coordenada(double Lat, double Lon);

public interface IAnaliseService
{
    Scores CalcularParaPato(Pato pato, Coordenada dsin);
    AnaliseDetalhe CalcularDetalhado(Pato pato, Coordenada dsin);
    Task<int> RecalcularTodosAsync(Coordenada dsin);
}
