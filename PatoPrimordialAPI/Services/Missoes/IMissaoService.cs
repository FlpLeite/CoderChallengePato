using PatoPrimordialAPI.Dtos.Missoes;

namespace PatoPrimordialAPI.Services.Missoes;

public interface IMissaoService
{
    MissaoDto CriarMissao(long patoId);
    Task<MissaoDto> IniciarAsync(long missaoId, CancellationToken cancellationToken);
    MissaoDto Abortar(long missaoId);
    MissaoDto? Obter(long missaoId);
    IReadOnlyCollection<MissaoListItemDto> Listar();
    MissaoTimelineDto Timeline(long missaoId, int page, int size);
}
