using Microsoft.AspNetCore.Mvc;
using PatoPrimordialAPI.Services.Missoes;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/regras")]
public class RegrasController(IMissaoCatalogoService catalogoService) : ControllerBase
{
    [HttpGet("taticas")]
    public IActionResult Taticas()
    {
        return Ok(catalogoService.ObterTaticas());
    }

    [HttpGet("defesas")]
    public IActionResult Defesas()
    {
        return Ok(catalogoService.ObterDefesas());
    }
}
