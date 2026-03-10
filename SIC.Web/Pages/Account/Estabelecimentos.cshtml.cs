using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC.Web.Services;

namespace SIC.Web.Pages.Account;

[Authorize]
public sealed class EstabelecimentosModel(SicAuthApiClient authApiClient) : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnGetChangeAsync(int estabelecimentoId, string? returnUrl, CancellationToken cancellationToken)
    {
        var usuarioId = TryGetIntClaim("sic_usuarioid");
        if (!usuarioId.HasValue)
        {
            return LocalRedirect("/Account/Login");
        }

        var isAdmin = string.Equals(User.FindFirst("sic_admin")?.Value, "1", StringComparison.OrdinalIgnoreCase);

        var result = await authApiClient.ChangeEstablishmentAsync(usuarioId.Value, isAdmin, estabelecimentoId, cancellationToken);
        if (result is null || !result.Success)
        {
            TempData["ErrorMessage"] = result?.Message ?? "Não foi possível alterar o estabelecimento.";
            return LocalRedirect(SafeReturn(returnUrl));
        }

        var list = await authApiClient.GetEstablishmentsAsync(usuarioId.Value, isAdmin, estabelecimentoId, cancellationToken);
        var selected = list.FirstOrDefault(x => x.EstabelecimentoId == estabelecimentoId);

        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity is not null)
        {
            ReplaceClaim(claimsIdentity, "sic_estabelecimentoid", estabelecimentoId.ToString());
            ReplaceClaim(claimsIdentity, "sic_estabelecimento_nome", selected?.NmEstabelecimento ?? "");

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        TempData["SuccessMessage"] = "Estabelecimento alterado com sucesso.";
        return LocalRedirect(SafeReturn(returnUrl));
    }

    private int? TryGetIntClaim(string claimType)
    {
        var value = User.FindFirst(claimType)?.Value;
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

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
}
