using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC.Web.Services;

namespace SIC.Web.Pages.Account;

[Authorize]
public sealed class AvatarModel(IWebHostEnvironment environment, SicAuthApiClient authApiClient) : PageModel
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostUploadAsync(IFormFile avatarFile, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetIntClaim("sic_usuarioid");
        if (!usuarioId.HasValue)
        {
            return new JsonResult(new { success = false, message = "Sessão inválida." });
        }

        if (avatarFile is null || avatarFile.Length == 0)
        {
            return new JsonResult(new { success = false, message = "Selecione uma imagem para upload." });
        }

        var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return new JsonResult(new { success = false, message = "Formato inválido. Utilize JPG ou PNG." });
        }

        if (avatarFile.Length > MaxFileSizeBytes)
        {
            return new JsonResult(new { success = false, message = "Arquivo excede o limite de 2MB." });
        }

        var uploadFolder = Path.Combine(environment.WebRootPath, "img", "upload");
        Directory.CreateDirectory(uploadFolder);

        var newFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadFolder, newFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await avatarFile.CopyToAsync(stream, cancellationToken);
        }

        var updateResult = await authApiClient.UpdateUserPhotoAsync(usuarioId.Value, newFileName, cancellationToken);
        if (updateResult is null || !updateResult.Success)
        {
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            return new JsonResult(new { success = false, message = updateResult?.Message ?? "Falha ao atualizar foto no banco." });
        }

        var oldFoto = User.FindFirst("sic_foto")?.Value;
        if (!string.IsNullOrWhiteSpace(oldFoto) && !string.Equals(oldFoto, newFileName, StringComparison.OrdinalIgnoreCase))
        {
            var oldPath = Path.Combine(uploadFolder, oldFoto);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity is not null)
        {
            ReplaceClaim(claimsIdentity, "sic_foto", newFileName);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        return new JsonResult(new
        {
            success = true,
            message = "Foto atualizada com sucesso.",
            imageUrl = Url.Content($"~/img/upload/{newFileName}")
        });
    }

    private int? TryGetIntClaim(string claimType)
    {
        var value = User.FindFirst(claimType)?.Value;
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static void ReplaceClaim(ClaimsIdentity identity, string claimType, string value)
    {
        var existing = identity.FindFirst(claimType);
        if (existing is not null)
        {
            identity.RemoveClaim(existing);
        }

        identity.AddClaim(new Claim(claimType, value));
    }
}
