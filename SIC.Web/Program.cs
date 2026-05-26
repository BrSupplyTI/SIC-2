using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SIC.Web.Services;
using SIC.Web.Services.Admin;
using SIC.Web.Services.Cotacao;
using SIC.Web.Services.PrePedidosPDF;
using SIC.Web.Services.Propostas;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<SicAuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<PedidoApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<ProdutoApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<ClienteApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<PrePedidoPDFApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<HomeApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<AdminApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var principal = context.Principal;
                if (principal?.Identity?.IsAuthenticated != true)
                {
                    return;
                }

                var usuarioIdClaim = principal.FindFirst("sic_usuarioid")?.Value;
                var sessionToken = principal.FindFirst("sic_session_token")?.Value;
                if (!int.TryParse(usuarioIdClaim, out var usuarioId) || string.IsNullOrWhiteSpace(sessionToken))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = context.HttpContext.Request.Headers.UserAgent.ToString();
                var authApiClient = context.HttpContext.RequestServices.GetRequiredService<SicAuthApiClient>();
                var validation = await authApiClient.ValidateSessionAsync(usuarioId, sessionToken, remoteIp, userAgent, context.HttpContext.RequestAborted);

                if (validation is null || !validation.Success)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
    })
    .AddOpenIdConnect("AzureAd", options =>
    {
        builder.Configuration.Bind("Authentication:AzureAd", options);
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        var hasClientSecret = !string.IsNullOrWhiteSpace(options.ClientSecret);

        if (hasClientSecret)
        {
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
        }
        else
        {
            // Fluxo sem client secret (somente ID token).
            // Requer habilitar "ID tokens (implicit and hybrid flows)" no App Registration.
            options.ResponseType = "id_token";
            options.SaveTokens = false;
            options.GetClaimsFromUserInfoEndpoint = false;
        }

        options.Scope.Add("email");

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                var email = principal?.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal?.FindFirst("preferred_username")?.Value
                    ?? principal?.FindFirst("email")?.Value;

                if (string.IsNullOrWhiteSpace(email))
                {
                    context.Fail("Não foi possível identificar o e-mail do usuário no token do Azure.");
                    return;
                }

                var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var authApiClient = context.HttpContext.RequestServices.GetRequiredService<SicAuthApiClient>();

                var authResult = await authApiClient.SsoLoginAsync(email, remoteIp, context.HttpContext.RequestAborted);
                if (authResult is null || !authResult.Success || authResult.User is null)
                {
                    var message = authResult?.Message ?? "Usuário não autorizado no SIC.";

                    if (string.Equals(authResult?.ErrorCode, "INVALID_CREDENTIALS", StringComparison.OrdinalIgnoreCase)
                        && message.Contains("não está vinculado ao SIC", StringComparison.OrdinalIgnoreCase))
                    {
                        message = $"{message} E-mail recebido no SSO: {email}";
                    }

                    context.Fail(message);
                    return;
                }

                if (principal?.Identity is not ClaimsIdentity identity)
                {
                    context.Fail("Identidade inválida no fluxo de autenticação.");
                    return;
                }

                identity.AddClaim(new Claim("sic_usuarioid", authResult.User.UsuarioId.ToString()));
                identity.AddClaim(new Claim("sic_login", authResult.User.Login));
                identity.AddClaim(new Claim("sic_nome", authResult.User.Nome));
                identity.AddClaim(new Claim("sic_admin", authResult.User.FlagAdmin ? "1" : "0"));

                if (authResult.User.EstabelecimentoId.HasValue)
                {
                    identity.AddClaim(new Claim("sic_estabelecimentoid", authResult.User.EstabelecimentoId.Value.ToString()));
                }

                if (!string.IsNullOrWhiteSpace(authResult.User.NmEstabelecimento))
                {
                    identity.AddClaim(new Claim("sic_estabelecimento_nome", authResult.User.NmEstabelecimento!));
                }

                if (!string.IsNullOrWhiteSpace(authResult.User.Foto))
                {
                    identity.AddClaim(new Claim("sic_foto", authResult.User.Foto));
                }

                if (!string.IsNullOrWhiteSpace(authResult.User.SessionToken))
                {
                    identity.AddClaim(new Claim("sic_session_token", authResult.User.SessionToken));
                }

                if (identity.FindFirst(ClaimTypes.Name) is null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, authResult.User.Nome));
                }
            },
            OnRemoteFailure = context =>
            {
                var error = Uri.EscapeDataString(context.Failure?.Message ?? "Falha no login SSO.");
                var basePath = context.Request.PathBase.Value ?? "";
                context.Response.Redirect($"{basePath}/Account/Login?erro={error}");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("sic_admin", "1"));
});
builder.Services.AddScoped<CotacaoEmailService>();

builder.Services.AddHttpClient<CotacaoApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl não configurado.");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddAuthorization();

var app = builder.Build();

// PathBase DEVE ser o primeiro middleware do pipeline.
// Ele remove o prefixo do Request.Path para que routing, auth, static files etc. funcionem corretamente.
var pathBase = builder.Configuration["AppSettings:PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

app.Run();
