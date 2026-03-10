using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC.Web.Services;

namespace SIC.Web.Pages.Account;

[Authorize]
public sealed class SecurityModel(SicAuthApiClient authApiClient) : PageModel
{
    private static readonly Regex PasswordRegex = new(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{7,}$", RegexOptions.Compiled);

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostChangePasswordAsync(string newPassword, string confirmPassword, CancellationToken cancellationToken)
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

        var result = await authApiClient.ChangePasswordAsync(usuarioId, newPassword, cancellationToken);
        return new JsonResult(new
        {
            success = result?.Success == true,
            message = result?.Message ?? "Não foi possível alterar a senha."
        });
    }
}
