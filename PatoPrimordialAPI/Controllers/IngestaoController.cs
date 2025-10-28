using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PatoPrimordialAPI.Dtos;
using PatoPrimordialAPI.Services;
using PatoPrimordialAPI.Services.Ingestao;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/ingestao")]
public class IngestaoController : ControllerBase
{
    private readonly IIngestaoService _ingestaoService;

    public IngestaoController(IIngestaoService ingestaoService)
    {
        _ingestaoService = ingestaoService;
    }

    [HttpPost("drone-avistamento")]
    public async Task<ActionResult<PatoDto>> RegistrarAvistamento([FromBody] DroneAvistamentoRequest request)
    {
        var pato = await _ingestaoService.RegistrarAvistamentoAsync(request);
        return Ok(pato);
    }
}
