using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Admin;
using SIC.Web.Services.Admin;

namespace SIC.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController(AdminApiClient adminApiClient) : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Administração";
        return View();
    }

    public async Task<IActionResult> MensagensImportantes(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Mensagens Importantes";
        var mensagens = await adminApiClient.GetAllNoticesAsync(cancellationToken);
        return View(mensagens);
    }

    [HttpPost]
    public async Task<IActionResult> ExpirarMensagem(int avisoId, CancellationToken cancellationToken)
    {
        var success = await adminApiClient.ExpireNoticeAsync(avisoId, cancellationToken);
        return Json(new { success });
    }

    [HttpPost]
    public async Task<IActionResult> ExcluirMensagem(int avisoId, CancellationToken cancellationToken)
    {
        var success = await adminApiClient.DeleteNoticeAsync(avisoId, cancellationToken);
        return Json(new { success });
    }

    [HttpPost]
    public async Task<IActionResult> CriarMensagem([FromBody] CreateNoticeVm model, CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        int.TryParse(usuarioIdClaim, out var usuarioResponsavelId);
        model.UsuarioResponsavelID = usuarioResponsavelId;

        var success = await adminApiClient.CreateNoticeAsync(model, cancellationToken);
        return Json(new { success });
    }

    [HttpGet]
    public async Task<IActionResult> GetAreas(CancellationToken cancellationToken)
    {
        var areas = await adminApiClient.GetAreasAsync(cancellationToken);
        return Json(areas);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsuarios(CancellationToken cancellationToken)
    {
        var usuarios = await adminApiClient.GetActiveUsersAsync(cancellationToken);
        return Json(usuarios);
    }
}
