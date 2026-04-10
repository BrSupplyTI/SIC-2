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

        var ok = await homeApi.AddUserShortcutAsync(usuarioId, request.AtalhoID, cancellationToken);
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
}

public sealed class RemoveShortcutRequest
{
    public int AtalhoID { get; set; }
}
