using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using SIC.Api.Models.Auth;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class SicAuthService : ISicAuthService
{
    private const int SessionTimeoutMinutes = 30;
    private const string ResetTokenCachePrefix = "reset-token:";
    private readonly string? _masterPassword;
    private readonly IMemoryCache _cache;
    private readonly int _resetTokenExpirationMinutes;
    private readonly bool _exposeTokenInResponse;
    private readonly string _updatePasswordSql;
    private readonly string _resetLinkBaseUrl;
    private readonly IEmailService _emailService;
    private readonly IAuthRepository _authRepository;

    public SicAuthService(IConfiguration configuration, IMemoryCache cache, IEmailService emailService, IAuthRepository authRepository)
    {
        _cache = cache;
        _emailService = emailService;
        _authRepository = authRepository;

        _masterPassword = configuration["LegacyAuth:MasterPassword"];
        _resetTokenExpirationMinutes = configuration.GetValue<int?>("PasswordReset:TokenExpirationMinutes") ?? 30;
        _exposeTokenInResponse = configuration.GetValue<bool?>("PasswordReset:ExposeTokenInResponse") ?? false;
        _updatePasswordSql = configuration["PasswordReset:UpdatePasswordSql"]
            ?? "UPDATE BrWeb..Permissoes_Intranet SET Senha = @novaSenha WHERE UsuarioID = @usuarioId;";
        _resetLinkBaseUrl = configuration["PasswordReset:ResetLinkBaseUrl"]
            ?? "https://localhost:7296/Account/ResetPassword";
    }

    public Task<AuthResult> LoginWithPasswordAsync(string login, string password, string remoteIp, string? userAgent, CancellationToken cancellationToken = default)
        => LoginInternalAsync(login, password, remoteIp, userAgent, isSso: false, cancellationToken);

    public Task<AuthResult> LoginWithSsoAsync(string email, string remoteIp, string? userAgent, CancellationToken cancellationToken = default)
        => LoginInternalAsync(email, null, remoteIp, userAgent, isSso: true, cancellationToken);

    public async Task<OperationResult> ValidateSessionAsync(int usuarioId, string sessionToken, string remoteIp, string? userAgent, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0 || string.IsNullOrWhiteSpace(sessionToken))
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_SESSION",
                Message = "Sessão inválida."
            };
        }

        var refreshed = await _authRepository.RefreshSessionAsync(usuarioId, sessionToken, SessionTimeoutMinutes, remoteIp, userAgent, cancellationToken);
        if (!refreshed)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "SESSION_EXPIRED",
                Message = "Sessão expirada ou inválida."
            };
        }

        return new OperationResult
        {
            Success = true,
            Message = "Sessão válida."
        };
    }

    public async Task<OperationResult> LogoutSessionAsync(int usuarioId, string sessionToken, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0 || string.IsNullOrWhiteSpace(sessionToken))
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_SESSION",
                Message = "Sessão inválida para logoff."
            };
        }

        var closed = await _authRepository.DeactivateSessionAsync(usuarioId, sessionToken, cancellationToken);
        return new OperationResult
        {
            Success = closed,
            ErrorCode = closed ? null : "SESSION_NOT_FOUND",
            Message = closed ? "Sessão encerrada com sucesso." : "Sessão não encontrada ou já encerrada."
        };
    }

    public async Task<OperationResult> RequestPasswordResetAsync(string identifier, string remoteIp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return new OperationResult
            {
                Success = true,
                Message = "Se o usuário existir, o procedimento de redefinição será iniciado."
            };
        }

        var user = await _authRepository.GetUserByIdentifierAsync(identifier, cancellationToken);
        if (user is null)
        {
            await _authRepository.InsertLogAsync(
                $"Esqueci minha senha solicitado para usuário inexistente: {identifier} | IP: {remoteIp}",
                0,
                cancellationToken);

            return new OperationResult
            {
                Success = true,
                Message = "Se o usuário existir, o procedimento de redefinição será iniciado."
            };
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_resetTokenExpirationMinutes);

        _cache.Set($"{ResetTokenCachePrefix}{token}", new PasswordResetTokenInfo
        {
            UsuarioId = user.UsuarioId,
            ExpiresAt = expiresAt
        }, expiresAt);

        await _authRepository.InsertLogAsync(
            $"Token de reset de senha gerado para usuário {user.UsuarioId} | IP: {remoteIp}",
            user.UsuarioId,
            cancellationToken);

        var emailDestino = user.Email ?? await _authRepository.GetActiveEmailByUsuarioIdAsync(user.UsuarioId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(emailDestino))
        {
            var resetLink = BuildResetLink(token);
            var htmlBody = $"""
                <p>Olá, {user.Nome}.</p>
                <p>Recebemos uma solicitação para redefinir sua senha no SIC.</p>
                <p><a href=\"{resetLink}\">Clique aqui para redefinir sua senha</a></p>
                <p>Este link expira em {_resetTokenExpirationMinutes} minutos.</p>
                <p>Se você não solicitou, ignore este e-mail.</p>
                """;

            try
            {
                await _emailService.SendAsync(emailDestino, "SIC - Redefinição de senha", htmlBody, cancellationToken);
            }
            catch (Exception ex)
            {
                await _authRepository.InsertLogAsync(
                    $"Falha ao enviar e-mail de redefinição para usuário {user.UsuarioId}: {ex.Message}",
                    user.UsuarioId,
                    cancellationToken);
            }
        }

        return new OperationResult
        {
            Success = true,
            Message = "Solicitação registrada. Utilize o token para redefinir a senha.",
            ResetToken = _exposeTokenInResponse ? token : null,
            ExpiresAt = _exposeTokenInResponse ? expiresAt : null
        };
    }

    public async Task<OperationResult> ResetPasswordAsync(string token, string newPassword, string remoteIp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "Token e nova senha são obrigatórios."
            };
        }

        if (newPassword.Length < 8)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "WEAK_PASSWORD",
                Message = "A nova senha deve conter pelo menos 8 caracteres."
            };
        }

        if (!_cache.TryGetValue<PasswordResetTokenInfo>($"{ResetTokenCachePrefix}{token}", out var tokenInfo) || tokenInfo is null)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_OR_EXPIRED_TOKEN",
                Message = "Token inválido ou expirado."
            };
        }

        var reset = await _authRepository.ResetPasswordAsync(tokenInfo.UsuarioId, newPassword, _updatePasswordSql, cancellationToken);
        if (!reset)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "PASSWORD_NOT_UPDATED",
                Message = "Não foi possível atualizar a senha. Verifique o SQL configurado em PasswordReset:UpdatePasswordSql."
            };
        }

        _cache.Remove($"{ResetTokenCachePrefix}{token}");

        await _authRepository.InsertLogAsync(
            $"Senha redefinida para usuário {tokenInfo.UsuarioId} | IP: {remoteIp}",
            tokenInfo.UsuarioId,
            cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Senha redefinida com sucesso."
        };
    }

    public async Task<IReadOnlyList<EstablishmentDto>> GetAuthorizedEstablishmentsAsync(int usuarioId, bool isAdmin, int? currentEstabelecimentoId, CancellationToken cancellationToken = default)
    {
        var establishments = await _authRepository.GetAuthorizedEstablishmentsAsync(usuarioId, isAdmin, cancellationToken);
        return establishments.Select(item => new EstablishmentDto
        {
            EstabelecimentoId = item.EstabelecimentoId,
            NmEstabelecimento = item.NmEstabelecimento,
            CdEstabelecimento = item.CdEstabelecimento,
            IsCurrent = currentEstabelecimentoId.HasValue && currentEstabelecimentoId.Value == item.EstabelecimentoId
        }).ToList();
    }

    public async Task<OperationResult> ChangeEstablishmentAsync(int usuarioId, bool isAdmin, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        var allowed = await _authRepository.IsAuthorizedForEstablishmentAsync(usuarioId, isAdmin, estabelecimentoId, cancellationToken);
        if (!allowed)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "NOT_AUTHORIZED_ESTABLISHMENT",
                Message = "Usuário não autorizado para o estabelecimento informado."
            };
        }

        var changed = await _authRepository.ChangeEstablishmentAsync(usuarioId, estabelecimentoId, cancellationToken);
        if (!changed)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "ESTABLISHMENT_NOT_CHANGED",
                Message = "Não foi possível trocar o estabelecimento."
            };
        }

        return new OperationResult
        {
            Success = true,
            Message = "Estabelecimento alterado com sucesso."
        };
    }

    public async Task<OperationResult> UpdateUserPhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0 || string.IsNullOrWhiteSpace(foto))
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "Parâmetros inválidos para atualizar a foto."
            };
        }

        var changed = await _authRepository.UpdateUserPhotoAsync(usuarioId, foto, cancellationToken);
        if (!changed)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "PHOTO_NOT_UPDATED",
                Message = "Não foi possível atualizar a foto do usuário."
            };
        }

        return new OperationResult
        {
            Success = true,
            Message = "Foto atualizada com sucesso."
        };
    }

    public async Task<OperationResult> ChangePasswordAsync(int usuarioId, string newPassword, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0 || string.IsNullOrWhiteSpace(newPassword))
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "Parâmetros inválidos para alteração de senha."
            };
        }

        var passwordPattern = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{7,}$");
        if (!passwordPattern.IsMatch(newPassword))
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "WEAK_PASSWORD",
                Message = "A senha deve conter no mínimo 7 caracteres, uma letra maiúscula, um número e um caractere especial."
            };
        }

        var changed = await _authRepository.ChangePasswordAsync(usuarioId, newPassword, cancellationToken);
        if (!changed)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "PASSWORD_NOT_CHANGED",
                Message = "Não foi possível alterar a senha."
            };
        }

        return new OperationResult
        {
            Success = true,
            Message = "Senha alterada com sucesso."
        };
    }

    private async Task<AuthResult> LoginInternalAsync(string identifier, string? password, string remoteIp, string? userAgent, bool isSso, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return new AuthResult { ErrorCode = "INVALID_INPUT", Message = "Credenciais inválidas." };
        }

        var user = isSso
            ? await _authRepository.GetUserByEmailAsync(identifier, cancellationToken)
            : await _authRepository.GetUserByPasswordAsync(identifier, password ?? string.Empty, _masterPassword, cancellationToken);

        if (user is null)
        {
            await _authRepository.InsertLogAsync(
                $"Usuário ou senha incorretos | Identificador: {identifier} | IP Tentativa: {remoteIp}",
                0,
                cancellationToken);

            return new AuthResult
            {
                Success = false,
                ErrorCode = "INVALID_CREDENTIALS",
                Message = isSso
                    ? "Usuário do Azure não está vinculado ao SIC."
                    : "Login ou senha incorretos."
            };
        }

        var sessionToken = Guid.NewGuid().ToString("N");
        var sessionCreated = await _authRepository.TryCreateSessionAsync(user.UsuarioId, sessionToken, SessionTimeoutMinutes, remoteIp, userAgent, cancellationToken);
        if (!sessionCreated)
        {
            return new AuthResult
            {
                Success = false,
                ErrorCode = "SESSION_CREATE_FAILED",
                Message = "Não foi possível iniciar a sessão do usuário."
            };
        }

        await _authRepository.UpdateLastLoginAsync(user.UsuarioId, cancellationToken);

        return new AuthResult
        {
            Success = true,
            User = new SicUserDto
            {
                UsuarioId = user.UsuarioId,
                Login = user.Login,
                Nome = user.Nome,
                Email = user.Email,
                FlagAdmin = user.FlagAdmin,
                FlagBackOffice = user.FlagBackOffice,
                EstabelecimentoId = user.EstabelecimentoId,
                NmEstabelecimento = user.NmEstabelecimento,
                ApelidoEstabelecimento = user.ApelidoEstabelecimento,
                Foto = user.Foto,
                SessionToken = sessionToken
            }
        };
    }

    private string BuildResetLink(string token)
    {
        var separator = _resetLinkBaseUrl.Contains('?') ? "&" : "?";
        return $"{_resetLinkBaseUrl}{separator}token={Uri.EscapeDataString(token)}";
    }


    private sealed class PasswordResetTokenInfo
    {
        public int UsuarioId { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
