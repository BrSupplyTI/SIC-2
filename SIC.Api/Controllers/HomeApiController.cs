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
        await service.AddUserShortcutAsync(request.UsuarioID, request.AtalhoID, request.Estilo, cancellationToken);
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

    [HttpGet("monitores")]
    public async Task<IActionResult> GetAllMonitors(CancellationToken cancellationToken)
    {
        var result = await service.GetAllMonitorsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("monitores/usuario/{usuarioId:int}")]
    public async Task<IActionResult> GetUserMonitorResults(int usuarioId, CancellationToken cancellationToken)
    {
        var result = await service.GetUserMonitorResultsAsync(usuarioId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("monitores/usuario")]
    public async Task<IActionResult> AddUserMonitor([FromBody] AddUserMonitorRequest request, CancellationToken cancellationToken)
    {
        await service.AddUserMonitorAsync(request.UsuarioID, request.MonitorID, request.Valor, cancellationToken);
        return NoContent();
    }

    [HttpDelete("monitores/usuario/{usuarioId:int}/{usuarioMonitorId:int}")]
    public async Task<IActionResult> RemoveUserMonitor(int usuarioId, int usuarioMonitorId, CancellationToken cancellationToken)
    {
        await service.RemoveUserMonitorAsync(usuarioId, usuarioMonitorId, cancellationToken);
        return NoContent();
    }
}

public sealed class AddUserMonitorRequest
{
    public int UsuarioID { get; set; }
    public int MonitorID { get; set; }
    public string Valor { get; set; } = string.Empty;
}

public sealed class AddUserShortcutRequest
{
    public int UsuarioID { get; set; }
    public int AtalhoID { get; set; }
    public string Estilo { get; set; } = string.Empty;
}
