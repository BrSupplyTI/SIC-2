using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC.Web.Services;

namespace SIC.Web.Pages.Account;

[AllowAnonymous]
public sealed class ForgotPasswordModel(SicAuthApiClient authApiClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Informe o login ou e-mail.")]
    public string Identifier { get; set; } = string.Empty;

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public bool Success { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await authApiClient.ForgotPasswordAsync(Identifier, cancellationToken);
        Success = result?.Success == true;
        Message = result?.Message ?? "Não foi possível solicitar a redefinição de senha.";

        return Page();
    }
}
