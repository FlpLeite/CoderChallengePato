using PatoPrimordialAPI.Domain.Missoes;

namespace PatoPrimordialAPI.Services.Missoes;

public interface IMissaoCatalogoService
{
    IReadOnlyCollection<RegraTatica> ObterTaticas();
    IReadOnlyCollection<RegraDefesa> ObterDefesas();
    IReadOnlyCollection<FraquezaCatalogo> ObterFraquezas();
}
