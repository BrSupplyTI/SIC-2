using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Profile;
using SIC.Web.Services;

namespace SIC.Web.Controllers;

[Authorize]
[Route("Account")]
public sealed class AccountController(IWebHostEnvironment environment, SicAuthApiClient apiClient) : Controller
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    [HttpGet("MeusDados")]
    public async Task<IActionResult> MeusDados(CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var profile = await apiClient.GetMyProfileAsync(usuarioId.Value, cancellationToken);
        if (profile is null)
        {
            TempData["MyDataError"] = "Não foi possível carregar os dados do usuário.";
            return View(new MyDataPageVm());
        }

        var areas = await apiClient.GetAreasAsync(cancellationToken);

        return View(new MyDataPageVm
        {
            UsuarioId = profile.UsuarioId,
            Nome = profile.Nome,
            Email = profile.Email,
            Telefone = profile.Telefone,
            AreaId = profile.AreaId,
            Foto = profile.Foto,
            Permissoes = profile.Permissoes,
            Areas = areas
        });
    }

    [HttpPost("MeusDados")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Atualizar([FromForm] MyDataPageVm model, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await apiClient.UpdateMyProfileAsync(new UpdateUserProfileVm
        {
            UsuarioId = usuarioId.Value,
            AreaId = model.AreaId,
            Telefone = model.Telefone
        }, cancellationToken);

        TempData[result?.Success == true ? "MyDataSuccess" : "MyDataError"] = result?.Message ?? "Falha ao atualizar dados.";
        return RedirectToAction(nameof(MeusDados));
    }

    [HttpPost("MeusDados/Foto")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarFoto(IFormFile foto, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        if (foto is null || foto.Length == 0)
        {
            TempData["MyDataError"] = "Selecione uma imagem para upload.";
            return RedirectToAction(nameof(MeusDados));
        }

        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            TempData["MyDataError"] = "Formato inválido. Utilize JPG ou PNG.";
            return RedirectToAction(nameof(MeusDados));
        }

        if (foto.Length > MaxFileSizeBytes)
        {
            TempData["MyDataError"] = "Arquivo excede o limite de 2MB.";
            return RedirectToAction(nameof(MeusDados));
        }

        var uploadFolder = Path.Combine(environment.WebRootPath, "img", "upload");
        Directory.CreateDirectory(uploadFolder);

        var oldFoto = User.FindFirst("sic_foto")?.Value;
        var newFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadFolder, newFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await foto.CopyToAsync(stream, cancellationToken);
        }

        var result = await apiClient.UpdateMyProfilePhotoAsync(usuarioId.Value, newFileName, cancellationToken);
        if (result is null || !result.Success)
        {
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            TempData["MyDataError"] = result?.Message ?? "Não foi possível atualizar a foto.";
            return RedirectToAction(nameof(MeusDados));
        }

        if (!string.IsNullOrWhiteSpace(oldFoto) && !string.Equals(oldFoto, newFileName, StringComparison.OrdinalIgnoreCase))
        {
            var oldPath = Path.Combine(uploadFolder, oldFoto);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        await UpdateFotoClaimAsync(newFileName);
        TempData["MyDataSuccess"] = "Foto atualizada com sucesso.";
        return RedirectToAction(nameof(MeusDados));
    }

    [HttpPost("MeusDados/Foto/Excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirFoto(CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        var oldFoto = User.FindFirst("sic_foto")?.Value;
        var result = await apiClient.RemoveMyProfilePhotoAsync(usuarioId.Value, cancellationToken);
        if (result is null || !result.Success)
        {
            TempData["MyDataError"] = result?.Message ?? "Não foi possível remover a foto.";
            return RedirectToAction(nameof(MeusDados));
        }

        if (!string.IsNullOrWhiteSpace(oldFoto))
        {
            var path = Path.Combine(environment.WebRootPath, "img", "upload", oldFoto);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        await UpdateFotoClaimAsync(null);
        TempData["MyDataSuccess"] = "Foto removida com sucesso.";
        return RedirectToAction(nameof(MeusDados));
    }

    private int? TryGetUsuarioId()
    {
        var claim = User.FindFirst("sic_usuarioid")?.Value;
        return int.TryParse(claim, out var usuarioId) ? usuarioId : null;
    }

    private async Task UpdateFotoClaimAsync(string? foto)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var current = identity.FindFirst("sic_foto");
        if (current is not null)
        {
            identity.RemoveClaim(current);
        }

        if (!string.IsNullOrWhiteSpace(foto))
        {
            identity.AddClaim(new Claim("sic_foto", foto));
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
