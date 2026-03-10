using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC.Web.Services;

namespace SIC.Web.Pages.Account;

public sealed class LogoutModel(SicAuthApiClient authApiClient) : PageModel
{
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        var sessionToken = User.FindFirst("sic_session_token")?.Value;

        if (int.TryParse(usuarioIdClaim, out var usuarioId) && !string.IsNullOrWhiteSpace(sessionToken))
        {
            _ = await authApiClient.LogoutSessionAsync(usuarioId, sessionToken, cancellationToken);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect("/Account/Login");
    }
}
