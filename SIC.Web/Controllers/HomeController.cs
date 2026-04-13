using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Home;
using SIC.Web.Services;

namespace SIC.Web.Controllers;

public sealed class HomeController(HomeApiClient homeApi) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "SIC - Portal Corporativo";

        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        if (int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            ViewBag.Atalhos = await homeApi.GetUserShortcutsAsync(usuarioId, cancellationToken);
            ViewBag.UsuarioID = usuarioId;
        }
        else
        {
            ViewBag.Atalhos = new List<ShortcutVm>();
            ViewBag.UsuarioID = 0;
        }

        ViewBag.Cotacoes = await homeApi.GetCurrencyQuotesAsync(cancellationToken);

        var estabelecimentoIdClaim = User.FindFirst("sic_estabelecimentoid")?.Value;
        if (int.TryParse(estabelecimentoIdClaim, out var estabelecimentoId))
            ViewBag.Clima = await homeApi.GetWeatherInfoAsync(estabelecimentoId, cancellationToken);

        return View();
    }

    [Authorize]
    [HttpGet("Home/Atalhos")]
    public async Task<IActionResult> GetAllShortcuts(CancellationToken cancellationToken)
    {
        var all = await homeApi.GetAllShortcutsAsync(cancellationToken);
        return Json(all);
    }

    [Authorize]
    [HttpPost("Home/Atalhos/Adicionar")]
    public async Task<IActionResult> AddShortcut([FromBody] AddShortcutRequest request, CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var ok = await homeApi.AddUserShortcutAsync(usuarioId, request.AtalhoID, request.Estilo, cancellationToken);
        return ok ? Ok() : StatusCode(500);
    }

    [Authorize]
    [HttpPost("Home/Atalhos/Remover")]
    public async Task<IActionResult> RemoveShortcut([FromBody] RemoveShortcutRequest request, CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var ok = await homeApi.RemoveUserShortcutAsync(usuarioId, request.AtalhoID, cancellationToken);
        return ok ? Ok() : StatusCode(500);
    }

    [Authorize]
    [HttpGet("Home/Monitores")]
    public async Task<IActionResult> GetAllMonitors(CancellationToken cancellationToken)
    {
        var all = await homeApi.GetAllMonitorsAsync(cancellationToken);
        return Json(all);
    }

    [Authorize]
    [HttpGet("Home/Monitores/Resultados")]
    public async Task<IActionResult> GetUserMonitorResults(CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var results = await homeApi.GetUserMonitorResultsAsync(usuarioId, cancellationToken);
        return Json(results);
    }

    [Authorize]
    [HttpPost("Home/Monitores/Adicionar")]
    public async Task<IActionResult> AddMonitor([FromBody] AddMonitorRequest request, CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var ok = await homeApi.AddUserMonitorAsync(usuarioId, request.MonitorID, request.Valor, cancellationToken);
        return ok ? Ok() : StatusCode(500);
    }

    [Authorize]
    [HttpPost("Home/Monitores/Remover")]
    public async Task<IActionResult> RemoveMonitor([FromBody] RemoveMonitorRequest request, CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var ok = await homeApi.RemoveUserMonitorAsync(usuarioId, request.UsuarioMonitorID, cancellationToken);
        return ok ? Ok() : StatusCode(500);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        ViewData["Title"] = "Privacy Policy";
        return View();
    }

    [AllowAnonymous]
    [Route("Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

public sealed class AddShortcutRequest
{
    public int AtalhoID { get; set; }
    public string Estilo { get; set; } = string.Empty;
}

public sealed class RemoveShortcutRequest
{
    public int AtalhoID { get; set; }
}

public sealed class AddMonitorRequest
{
    public int MonitorID { get; set; }
    public string Valor { get; set; } = string.Empty;
}

public sealed class RemoveMonitorRequest
{
    public int UsuarioMonitorID { get; set; }
}
