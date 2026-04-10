using Microsoft.AspNetCore.Mvc;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/home")]
public sealed class HomeApiController(IHomeService service) : ControllerBase
{
    [HttpGet("atalhos/usuario/{usuarioId:int}")]
    public async Task<IActionResult> GetUserShortcuts(int usuarioId, CancellationToken cancellationToken)
    {
        var result = await service.GetUserShortcutsAsync(usuarioId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("atalhos")]
    public async Task<IActionResult> GetAllShortcuts(CancellationToken cancellationToken)
    {
        var result = await service.GetAllShortcutsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("atalhos/usuario")]
    public async Task<IActionResult> AddUserShortcut([FromBody] AddUserShortcutRequest request, CancellationToken cancellationToken)
    {
        await service.AddUserShortcutAsync(request.UsuarioID, request.AtalhoID, cancellationToken);
        return NoContent();
    }

    [HttpDelete("atalhos/usuario/{usuarioId:int}/{atalhoId:int}")]
    public async Task<IActionResult> RemoveUserShortcut(int usuarioId, int atalhoId, CancellationToken cancellationToken)
    {
        await service.RemoveUserShortcutAsync(usuarioId, atalhoId, cancellationToken);
        return NoContent();
    }

    [HttpGet("cotacoes")]
    public async Task<IActionResult> GetCurrencyQuotes(CancellationToken cancellationToken)
    {
        var result = await service.GetCurrencyQuotesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("clima/{estabelecimentoId:int}")]
    public async Task<IActionResult> GetWeatherInfo(int estabelecimentoId, CancellationToken cancellationToken)
    {
        var result = await service.GetWeatherInfoAsync(estabelecimentoId, cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }
}

public sealed class AddUserShortcutRequest
{
    public int UsuarioID { get; set; }
    public int AtalhoID { get; set; }
}
