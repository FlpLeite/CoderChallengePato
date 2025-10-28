using Microsoft.AspNetCore.Mvc;
using PatoPrimordialAPI.Services.Missoes;

namespace PatoPrimordialAPI.Controllers;

[ApiController]
[Route("api/catalogos")]
public class CatalogosController(IMissaoCatalogoService catalogoService) : ControllerBase
{
    [HttpGet("fraquezas")]
    public IActionResult Fraquezas()
    {
        return Ok(catalogoService.ObterFraquezas());
    }
}
