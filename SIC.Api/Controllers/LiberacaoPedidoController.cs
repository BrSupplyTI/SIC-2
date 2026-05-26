using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Liberacao;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/liberacao-pedidos")]
public sealed class LiberacaoPedidoController(ILiberacaoPedidoService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int estabelecimentoId,
        [FromQuery] int usuarioId,
        [FromQuery] LiberacaoPedidoFilterDto filtro,
        CancellationToken cancellationToken)
    {
        if (estabelecimentoId <= 0 || usuarioId <= 0)
            return BadRequest("EstabelecimentoId e UsuarioId são obrigatórios.");

        var result = await service.ListarAsync(estabelecimentoId, usuarioId, filtro, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{cotacaoId:int}/detalhes")]
    public async Task<IActionResult> ObterDetalhes(int cotacaoId, CancellationToken cancellationToken)
    {
        if (cotacaoId <= 0)
            return BadRequest("CotacaoId é obrigatório.");

        var result = await service.ObterDetalhesAsync(cotacaoId, cancellationToken);
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{cotacaoId:int}/analise")]
    public async Task<IActionResult> Analisar(
        int cotacaoId,
        [FromQuery] int usuarioId,
        CancellationToken cancellationToken)
    {
        if (cotacaoId <= 0 || usuarioId <= 0)
            return BadRequest("CotacaoId e UsuarioId são obrigatórios.");

        var result = await service.AnalisarAsync(cotacaoId, usuarioId, cancellationToken);
        return Ok(result);
    }
}
