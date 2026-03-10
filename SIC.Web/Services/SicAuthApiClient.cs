using System.Net.Http.Json;
using System.Text.Json;
using SIC.Web.Models.Auth;

namespace SIC.Web.Services;

public sealed class SicAuthApiClient(HttpClient httpClient)
{
    public Task<AuthApiResultVm?> PasswordLoginAsync(string login, string password, string remoteIp, CancellationToken cancellationToken = default)
        => SendAsync("api/auth/login", new LoginRequestVm
        {
            Login = login,
            Password = password,
            RemoteIp = remoteIp
        }, cancellationToken);

    public Task<AuthApiResultVm?> SsoLoginAsync(string email, string remoteIp, CancellationToken cancellationToken = default)
        => SendAsync("api/auth/sso-login", new SsoLoginRequestVm
        {
            Email = email,
            RemoteIp = remoteIp
        }, cancellationToken);

    public Task<OperationResultVm?> ValidateSessionAsync(int usuarioId, string sessionToken, string remoteIp, string? userAgent, CancellationToken cancellationToken = default)
        => SendOperationAsync("api/auth/validate-session", new
        {
            UsuarioId = usuarioId,
            SessionToken = sessionToken,
            RemoteIp = remoteIp,
            UserAgent = userAgent
        }, cancellationToken);

    public Task<OperationResultVm?> LogoutSessionAsync(int usuarioId, string sessionToken, CancellationToken cancellationToken = default)
        => SendOperationAsync("api/auth/logout-session", new
        {
            UsuarioId = usuarioId,
            SessionToken = sessionToken
        }, cancellationToken);

    public Task<OperationResultVm?> ForgotPasswordAsync(string identifier, CancellationToken cancellationToken = default)
        => SendOperationAsync("api/auth/forgot-password", new { Identifier = identifier }, cancellationToken);

    public Task<OperationResultVm?> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
        => SendOperationAsync("api/auth/reset-password", new ResetPasswordRequestVm
        {
            Token = token,
            NewPassword = newPassword
        }, cancellationToken);

    public async Task<IReadOnlyList<EstablishmentVm>> GetEstablishmentsAsync(int usuarioId, bool isAdmin, int? currentEstabelecimentoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/establishments", new EstablishmentListRequestVm
            {
                UsuarioId = usuarioId,
                IsAdmin = isAdmin,
                CurrentEstabelecimentoId = currentEstabelecimentoId
            }, cancellationToken);

            var data = await response.Content.ReadFromJsonAsync<List<EstablishmentVm>>(cancellationToken: cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public Task<OperationResultVm?> ChangeEstablishmentAsync(int usuarioId, bool isAdmin, int estabelecimentoId, CancellationToken cancellationToken = default)
        => SendOperationAsync("api/auth/change-establishment", new ChangeEstablishmentRequestVm
        {
            UsuarioId = usuarioId,
            IsAdmin = isAdmin,
            EstabelecimentoId = estabelecimentoId
        }, cancellationToken);

    public Task<OperationResultVm?> UpdateUserPhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default)
        => SendOperationAsync("api/auth/update-photo", new UpdateUserPhotoRequestVm
        {
            UsuarioId = usuarioId,
            Foto = foto
        }, cancellationToken);

    public Task<OperationResultVm?> ChangePasswordAsync(int usuarioId, string newPassword, CancellationToken cancellationToken = default)
        => SendOperationAsync("api/auth/change-password", new ChangePasswordRequestVm
        {
            UsuarioId = usuarioId,
            NewPassword = newPassword
        }, cancellationToken);

    private async Task<AuthApiResultVm?> SendAsync<TRequest>(string path, TRequest payload, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(path, payload, cancellationToken);
            return await ReadResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return new AuthApiResultVm
            {
                Success = false,
                ErrorCode = "API_UNAVAILABLE",
                Message = "Não foi possível conectar na API do SIC. Verifique se o SIC.Api está em execução e a URL configurada."
            };
        }
        catch (TaskCanceledException)
        {
            return new AuthApiResultVm
            {
                Success = false,
                ErrorCode = "API_TIMEOUT",
                Message = "Tempo de conexão com a API excedido."
            };
        }
    }

    private async Task<OperationResultVm?> SendOperationAsync<TRequest>(string path, TRequest payload, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(path, payload, cancellationToken);

            if (response.Content.Headers.ContentLength == 0)
            {
                return new OperationResultVm
                {
                    Success = false,
                    ErrorCode = "EMPTY_RESPONSE",
                    Message = $"Falha na operação. Status HTTP {(int)response.StatusCode}."
                };
            }

            return await response.Content.ReadFromJsonAsync<OperationResultVm>(cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            return new OperationResultVm
            {
                Success = false,
                ErrorCode = "API_UNAVAILABLE",
                Message = "Não foi possível conectar na API do SIC."
            };
        }
    }

    private static async Task<AuthApiResultVm?> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength == 0)
        {
            return new AuthApiResultVm
            {
                Success = false,
                Message = $"Falha ao autenticar. Status HTTP {(int)response.StatusCode}."
            };
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "application/problem+json", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(contentType))
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<AuthApiResultVm>(cancellationToken: cancellationToken);
            }
            catch (JsonException)
            {
                // fallback para texto simples abaixo
            }
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return new AuthApiResultVm
        {
            Success = false,
            ErrorCode = "API_INVALID_RESPONSE",
            Message = string.IsNullOrWhiteSpace(payload)
                ? $"Resposta inválida da API. Status HTTP {(int)response.StatusCode}."
                : payload.Length > 500 ? payload[..500] : payload
        };
    }
}
