using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC.Web.Services;

namespace SIC.Web.Pages.Account;

[AllowAnonymous]
public sealed class ResetPasswordModel(SicAuthApiClient authApiClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Informe a nova senha.")]
    [MinLength(8, ErrorMessage = "A nova senha deve ter no mínimo 8 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool Success { get; set; }

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            Message = "Token de redefinição inválido.";
            Success = false;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await authApiClient.ResetPasswordAsync(Token, NewPassword, cancellationToken);
        Success = result?.Success == true;
        Message = result?.Message ?? "Não foi possível redefinir a senha.";

        return Page();
    }
}
