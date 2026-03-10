using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SIC.Web.Services;

namespace SIC.Web.Pages.Account;

public sealed class LoginModel(SicAuthApiClient authApiClient) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet([FromQuery(Name = "erro")] string? erro = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(ResolveReturnUrl(ReturnUrl));
        }

        if (!string.IsNullOrWhiteSpace(erro))
        {
            ErrorMessage = erro;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var authResult = await authApiClient.PasswordLoginAsync(Input.Login, Input.Password, remoteIp, cancellationToken);

        if (authResult is null || !authResult.Success || authResult.User is null)
        {
            ErrorMessage = authResult?.ErrorCode == "SESSION_LOCKED"
                ? $"Sessão bloqueada. IP ativo: {authResult.ExistingIp}."
                : authResult?.Message ?? "Não foi possível autenticar no SIC.";

            return Page();
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

        return LocalRedirect(ResolveReturnUrl(ReturnUrl));
    }

    public IActionResult OnPostAzure()
    {
        var returnUrl = ResolveReturnUrl(ReturnUrl);

        return Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            "AzureAd");
    }

    private static string ResolveReturnUrl(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
            ? returnUrl
            : "/";

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Informe o login.")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        public string Password { get; set; } = string.Empty;
    }
}
