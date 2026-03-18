using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Account;
using SIC.Web.Models.Profile;
using SIC.Web.Services;

namespace SIC.Web.Controllers;

[Authorize]
[Route("Account")]
public sealed class AccountController(IWebHostEnvironment environment, SicAuthApiClient apiClient) : Controller
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;
    private static readonly Regex PasswordRegex = new(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{7,}$", RegexOptions.Compiled);

    [AllowAnonymous]
    [HttpGet("Login")]
    public IActionResult Login([FromQuery(Name = "erro")] string? erro = null, [FromQuery] string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(ResolveReturnUrl(returnUrl));
        }

        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl,
            ErrorMessage = string.IsNullOrWhiteSpace(erro) ? null : erro
        };

        return View(model);
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginPost([FromForm] LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Login", model);
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var authResult = await apiClient.PasswordLoginAsync(model.Input.Login, model.Input.Password, remoteIp, cancellationToken);

        if (authResult is null || !authResult.Success || authResult.User is null)
        {
            model.ErrorMessage = authResult?.ErrorCode == "SESSION_LOCKED"
                ? $"Sessão bloqueada. IP ativo: {authResult.ExistingIp}."
                : authResult?.Message ?? "Não foi possível autenticar no SIC.";

            return View("Login", model);
        }

        var claims = new List<Claim>
        {
            new("sic_usuarioid", authResult.User.UsuarioId.ToString()),
            new("sic_login", authResult.User.Login),
            new("sic_nome", authResult.User.Nome),
            new("sic_admin", authResult.User.FlagAdmin ? "1" : "0"),
            new(ClaimTypes.Name, authResult.User.Nome)
        };

        if (authResult.User.EstabelecimentoId.HasValue)
        {
            claims.Add(new Claim("sic_estabelecimentoid", authResult.User.EstabelecimentoId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(authResult.User.NmEstabelecimento))
        {
            claims.Add(new Claim("sic_estabelecimento_nome", authResult.User.NmEstabelecimento!));
        }

        if (!string.IsNullOrWhiteSpace(authResult.User.Foto))
        {
            claims.Add(new Claim("sic_foto", authResult.User.Foto));
        }

        if (!string.IsNullOrWhiteSpace(authResult.User.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, authResult.User.Email));
        }

        if (!string.IsNullOrWhiteSpace(authResult.User.SessionToken))
        {
            claims.Add(new Claim("sic_session_token", authResult.User.SessionToken));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return LocalRedirect(ResolveReturnUrl(model.ReturnUrl));
    }

    [AllowAnonymous]
    [HttpPost("LoginAzure")]
    [ValidateAntiForgeryToken]
    public IActionResult LoginAzure([FromForm] LoginViewModel model)
    {
        var returnUrl = ResolveReturnUrl(model.ReturnUrl);
        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "AzureAd");
    }

    [AllowAnonymous]
    [HttpGet("ForgotPassword")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous]
    [HttpPost("ForgotPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPasswordPost([FromForm] ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("ForgotPassword", model);
        }

        var result = await apiClient.ForgotPasswordAsync(model.Identifier, cancellationToken);
        model.Success = result?.Success == true;
        model.Message = result?.Message ?? "Não foi possível solicitar a redefinição de senha.";
        return View("ForgotPassword", model);
    }

    [AllowAnonymous]
    [HttpGet("ResetPassword")]
    public IActionResult ResetPassword([FromQuery] string token)
    {
        var model = new ResetPasswordViewModel { Token = token ?? string.Empty };
        if (string.IsNullOrWhiteSpace(model.Token))
        {
            model.Message = "Token de redefinição inválido.";
            model.Success = false;
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpPost("ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPasswordPost([FromForm] ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("ResetPassword", model);
        }

        var result = await apiClient.ResetPasswordAsync(model.Token, model.NewPassword, cancellationToken);
        model.Success = result?.Success == true;
        model.Message = result?.Message ?? "Não foi possível redefinir a senha.";
        return View("ResetPassword", model);
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        var sessionToken = User.FindFirst("sic_session_token")?.Value;

        if (int.TryParse(usuarioIdClaim, out var usuarioId) && !string.IsNullOrWhiteSpace(sessionToken))
        {
            _ = await apiClient.LogoutSessionAsync(usuarioId, sessionToken, cancellationToken);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("EstabelecimentosChange")]
    public async Task<IActionResult> ChangeEstabelecimento(int estabelecimentoId, string? returnUrl, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        var isAdmin = string.Equals(User.FindFirst("sic_admin")?.Value, "1", StringComparison.OrdinalIgnoreCase);
        var result = await apiClient.ChangeEstablishmentAsync(usuarioId.Value, isAdmin, estabelecimentoId, cancellationToken);
        if (result is null || !result.Success)
        {
            TempData["ErrorMessage"] = result?.Message ?? "Não foi possível alterar o estabelecimento.";
            return LocalRedirect(SafeReturn(returnUrl));
        }

        var list = await apiClient.GetEstablishmentsAsync(usuarioId.Value, isAdmin, estabelecimentoId, cancellationToken);
        var selected = list.FirstOrDefault(x => x.EstabelecimentoId == estabelecimentoId);

        if (User.Identity is ClaimsIdentity claimsIdentity)
        {
            ReplaceClaim(claimsIdentity, "sic_estabelecimentoid", estabelecimentoId.ToString());
            ReplaceClaim(claimsIdentity, "sic_estabelecimento_nome", selected?.NmEstabelecimento ?? string.Empty);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        TempData["SuccessMessage"] = "Estabelecimento alterado com sucesso.";
        return LocalRedirect(SafeReturn(returnUrl));
    }
    
    [HttpPost("AvatarUpload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile avatarFile, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
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

        var updateResult = await apiClient.UpdateUserPhotoAsync(usuarioId.Value, newFileName, cancellationToken);
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

        if (User.Identity is ClaimsIdentity identity)
        {
            ReplaceClaim(identity, "sic_foto", newFileName);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        return new JsonResult(new
        {
            success = true,
            message = "Foto atualizada com sucesso.",
            imageUrl = Url.Content($"~/img/upload/{newFileName}")
        });
    }

    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword, CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return new JsonResult(new { success = false, message = "Sessão inválida." });
        }

        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return new JsonResult(new { success = false, message = "Informe e confirme a nova senha." });
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            return new JsonResult(new { success = false, message = "A confirmação da senha não confere." });
        }

        if (!PasswordRegex.IsMatch(newPassword))
        {
            return new JsonResult(new { success = false, message = "A senha deve conter no mínimo 7 caracteres, uma letra maiúscula, um número e um caractere especial." });
        }

        var result = await apiClient.ChangePasswordAsync(usuarioId, newPassword, cancellationToken);
        return new JsonResult(new
        {
            success = result?.Success == true,
            message = result?.Message ?? "Não foi possível alterar a senha."
        });
    }

    [HttpGet("MeusDados")]
    public async Task<IActionResult> MeusDados(CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        var profile = await apiClient.GetMyProfileAsync(usuarioId.Value, cancellationToken);
        if (profile is null)
        {
            TempData["MyDataError"] = "Não foi possível carregar os dados do usuário.";
            return View(new MyDataPageVm());
        }

        var areas = await apiClient.GetAreasAsync(cancellationToken);
        profile.Areas = areas;
        return View(profile);
    }

    [HttpPost("MeusDados")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Atualizar([FromForm] MyDataPageVm model, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToAction(nameof(Login));
        }

        model.UsuarioId = usuarioId.Value;

        if (!ModelState.IsValid)
        {
            var areas = await apiClient.GetAreasAsync(cancellationToken);
            model.Areas = areas;

            var fullProfile = await apiClient.GetMyProfileAsync(usuarioId.Value, cancellationToken);
            model.Nome = fullProfile?.Nome ?? string.Empty;
            model.Email = fullProfile?.Email;
            model.Foto = fullProfile?.Foto;
            model.Permissoes = fullProfile?.Permissoes ?? [];

            return View("MeusDados", model);
        }

        var result = await apiClient.UpdateMyProfileAsync(model, cancellationToken);

        TempData[result?.Success == true ? "MyDataSuccess" : "MyDataError"] = result?.Message ?? "Falha ao atualizar dados.";
        return RedirectToAction(nameof(MeusDados));
    }

    [HttpPost("Foto")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarFoto(IFormFile foto, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToAction(nameof(Login));
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

    [HttpPost("FotoExcluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirFoto(CancellationToken cancellationToken)
    {
        var usuarioId = TryGetUsuarioId();
        if (!usuarioId.HasValue)
        {
            return RedirectToAction(nameof(Login));
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

    private static string ResolveReturnUrl(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
            ? returnUrl
            : "/";

    private static string SafeReturn(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
            ? returnUrl
            : "/";

    private static void ReplaceClaim(ClaimsIdentity identity, string claimType, string value)
    {
        var existing = identity.FindFirst(claimType);
        if (existing is not null)
        {
            identity.RemoveClaim(existing);
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            identity.AddClaim(new Claim(claimType, value));
        }
    }

    private async Task UpdateFotoClaimAsync(string? foto)
    {
        if (User.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        ReplaceClaim(identity, "sic_foto", foto ?? string.Empty);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
