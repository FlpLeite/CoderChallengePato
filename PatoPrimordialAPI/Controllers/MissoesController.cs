using Microsoft.AspNetCore.Mvc;
using PatoPrimordialAPI.Dtos.Missoes;
using PatoPrimordialAPI.Services.Missoes;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/missoes")]
public class MissoesController(IMissaoService missaoService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<MissaoListItemDto>> Listar()
    {
        return Ok(missaoService.Listar());
    }

    [HttpPost]
    public ActionResult<MissaoDto> Criar([FromBody] CriarMissaoRequest request)
    {
        if (request is null || request.PatoId <= 0)
        {
            return BadRequest("patoId é obrigatório");
        }

        try
        {
            var missao = missaoService.CriarMissao(request.PatoId);
            return CreatedAtAction(nameof(Obter), new { id = missao.Id }, missao);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id:long}/iniciar")]
    public async Task<ActionResult<MissaoDto>> Iniciar(long id, CancellationToken cancellationToken)
    {
        try
        {
            var missao = await missaoService.IniciarAsync(id, cancellationToken);
            return Ok(missao);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id:long}/abortar")]
    public ActionResult<MissaoDto> Abortar(long id)
    {
        try
        {
            var missao = missaoService.Abortar(id);
            return Ok(missao);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    public ActionResult<MissaoDto> Obter(long id)
    {
        var missao = missaoService.Obter(id);
        if (missao is null)
        {
            return NotFound();
        }

        return Ok(missao);
    }

    [HttpGet("{id:long}/timeline")]
    public ActionResult<MissaoTimelineDto> Timeline(long id, [FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        try
        {
            var timeline = missaoService.Timeline(id, page, size);
            return Ok(timeline);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
