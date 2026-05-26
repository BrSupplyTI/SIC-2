using Microsoft.AspNetCore.Mvc;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/permissoes")]
public sealed class PermissoesController(IPermissaoService service) : ControllerBase
{
    /// <summary>
    /// Verifica se um usuário possui uma permissão.
    /// GET /api/permissoes/{usuarioId}/{permissaoId}?flagAdmin=false
    /// </summary>
    [HttpGet("{usuarioId:int}/{permissaoId:int}")]
    public async Task<IActionResult> TemPermissao(
        int usuarioId,
        int permissaoId,
        [FromQuery] bool flagAdmin,
        CancellationToken cancellationToken)
    {
        if (usuarioId <= 0 || permissaoId <= 0)
            return BadRequest("UsuarioId e PermissaoId são obrigatórios.");

        var tem = await service.TemPermissaoAsync(usuarioId, permissaoId, flagAdmin, cancellationToken);
        return Ok(new PermissaoResultadoDto(tem));
    }
}

public sealed record PermissaoResultadoDto(bool TemPermissao);
