using System.Data;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Data.SqlClient;
using SIC.Api.Models.Auth;

namespace SIC.Api.Services;

public sealed class SicAuthService : ISicAuthService
{
    private static readonly HashSet<int> IgnoredUserIdsForLastLoginUpdate = [685, 1671];
    private const int SessionTimeoutMinutes = 30;
    private const string ResetTokenCachePrefix = "reset-token:";
    private readonly string _connectionString;
    private readonly string? _masterPassword;
    private readonly IMemoryCache _cache;
    private readonly int _resetTokenExpirationMinutes;
    private readonly bool _exposeTokenInResponse;
    private readonly string _updatePasswordSql;
    private readonly string _resetLinkBaseUrl;
    private readonly IEmailService _emailService;

    public SicAuthService(IConfiguration configuration, IMemoryCache cache, IEmailService emailService)
    {
        _cache = cache;
        _emailService = emailService;
        _connectionString = configuration.GetConnectionString("SicDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var refreshed = await RefreshSessionAsync(connection, usuarioId, sessionToken, remoteIp, userAgent, cancellationToken);
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var closed = await DeactivateSessionAsync(connection, usuarioId, sessionToken, cancellationToken);
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var user = await GetUserByIdentifierAsync(connection, identifier, cancellationToken);
        if (user is null)
        {
            await InsertLogAsync(connection,
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

        await InsertLogAsync(connection,
            $"Token de reset de senha gerado para usuário {user.UsuarioId} | IP: {remoteIp}",
            user.UsuarioId,
            cancellationToken);

        var emailDestino = user.Email ?? await GetActiveEmailByUsuarioIdAsync(connection, user.UsuarioId, cancellationToken);
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
                await InsertLogAsync(connection,
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(_updatePasswordSql, connection);
        cmd.Parameters.AddWithValue("@novaSenha", newPassword);
        cmd.Parameters.AddWithValue("@usuarioId", tokenInfo.UsuarioId);

        var affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows <= 0)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "PASSWORD_NOT_UPDATED",
                Message = "Não foi possível atualizar a senha. Verifique o SQL configurado em PasswordReset:UpdatePasswordSql."
            };
        }

        _cache.Remove($"{ResetTokenCachePrefix}{token}");

        await InsertLogAsync(connection,
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
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = isAdmin
            ? """
                SELECT E.EstabelecimentoID, E.NmEstabelecimento, E.CdEstabelecimento
                FROM BR_Estabelecimento E WITH (NOLOCK)
                WHERE E.FlagAtivo = 1
                  AND E.SerieNF > 0
                  AND E.OrdemExibicao > 0
                ORDER BY E.OrdemExibicao ASC;
                """
            : """
                SELECT E.EstabelecimentoID, E.NmEstabelecimento, E.CdEstabelecimento
                FROM BR_Estabelecimento E WITH (NOLOCK)
                WHERE E.FlagAtivo = 1
                  AND E.SerieNF > 0
                  AND E.OrdemExibicao > 0
                  AND E.EstabelecimentoID IN (
                      SELECT X.EstabelecimentoID
                      FROM BR_UsuarioEstabelecimento X WITH (NOLOCK)
                      WHERE X.UsuarioID = @usuarioId
                  )
                ORDER BY E.OrdemExibicao ASC;
                """;

        await using var cmd = new SqlCommand(sql, connection);
        if (!isAdmin)
        {
            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        }

        var result = new List<EstablishmentDto>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var estabelecimentoId = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID"));
            result.Add(new EstablishmentDto
            {
                EstabelecimentoId = estabelecimentoId,
                NmEstabelecimento = reader.GetString(reader.GetOrdinal("NmEstabelecimento")),
                CdEstabelecimento = ReadNullableString(reader, "CdEstabelecimento"),
                IsCurrent = currentEstabelecimentoId.HasValue && currentEstabelecimentoId.Value == estabelecimentoId
            });
        }

        return result;
    }

    public async Task<OperationResult> ChangeEstablishmentAsync(int usuarioId, bool isAdmin, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        var allowed = await IsAuthorizedForEstablishmentAsync(usuarioId, isAdmin, estabelecimentoId, cancellationToken);
        if (!allowed)
        {
            return new OperationResult
            {
                Success = false,
                ErrorCode = "NOT_AUTHORIZED_ESTABLISHMENT",
                Message = "Usuário não autorizado para o estabelecimento informado."
            };
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE BR_Usuario
            SET EstabelecimentoID = @estabelecimentoId
            WHERE UsuarioID = @usuarioId;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@estabelecimentoId", estabelecimentoId);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (rows <= 0)
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE BR_Usuario
            SET Foto = @foto
            WHERE UsuarioID = @usuarioId;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@foto", foto);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (rows <= 0)
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE BR_Usuario
            SET Senha = @senha
            WHERE UsuarioID = @usuarioId;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@senha", newPassword);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (rows <= 0)
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var user = isSso
            ? await GetUserByEmailAsync(connection, identifier, cancellationToken)
            : await GetUserByPasswordAsync(connection, identifier, password ?? string.Empty, cancellationToken);

        if (user is null)
        {
            await InsertLogAsync(connection,
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

        var sessionToken = await CreateSessionAsync(connection, user.UsuarioId, remoteIp, userAgent, cancellationToken);

        await UpdateLastLoginAsync(connection, user.UsuarioId, cancellationToken);

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
                EstabelecimentoId = user.EstabelecimentoId,
                NmEstabelecimento = user.NmEstabelecimento,
                ApelidoEstabelecimento = user.ApelidoEstabelecimento,
                Foto = user.Foto,
                SessionToken = sessionToken
            }
        };
    }

    private async Task<ViewLoginRow?> GetUserByPasswordAsync(SqlConnection connection, string login, string password, CancellationToken cancellationToken)
    {
        var emailColumnExists = await DoesColumnExistAsync(connection, "view_login", "Email", cancellationToken);

        var sql = $"""
            SELECT TOP 1
                UsuarioID,
                Login,
                Nome,
                FlagAdmin,
                {(emailColumnExists ? "Email" : "CAST(NULL AS NVARCHAR(256)) AS Email")},
                (SELECT TOP 1 U.Foto FROM BR_Usuario U WITH (NOLOCK) WHERE U.UsuarioID = view_login.UsuarioID) AS Foto,
                EstabelecimentoID,
                NmEstabelecimento,
                ApelidoEstabelecimento
            FROM view_login
            WHERE Login = @login
              AND (Senha = @senha OR (@masterPassword IS NOT NULL AND @senha = @masterPassword));
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@senha", password);
        cmd.Parameters.AddWithValue("@masterPassword", (object?)_masterPassword ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapViewLoginRow(reader);
    }

    private async Task<ViewLoginRow?> GetUserByIdentifierAsync(SqlConnection connection, string identifier, CancellationToken cancellationToken)
    {
        var emailColumnExists = await DoesColumnExistAsync(connection, "view_login", "Email", cancellationToken);

        var sql = $"""
            SELECT TOP 1
                UsuarioID,
                Login,
                Nome,
                FlagAdmin,
                {(emailColumnExists ? "Email" : "CAST(NULL AS NVARCHAR(256)) AS Email")},
                (SELECT TOP 1 U.Foto FROM BR_Usuario U WITH (NOLOCK) WHERE U.UsuarioID = view_login.UsuarioID) AS Foto,
                EstabelecimentoID,
                NmEstabelecimento,
                ApelidoEstabelecimento
            FROM view_login
            WHERE Login = @identifier
               {(emailColumnExists ? "OR LOWER(Email) = LOWER(@identifier)" : string.Empty)};
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@identifier", identifier);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapViewLoginRow(reader);
    }

    private async Task<ViewLoginRow?> GetUserByEmailAsync(SqlConnection connection, string email, CancellationToken cancellationToken)
    {
        var activeUser = await GetActiveUserByEmailAsync(connection, email, cancellationToken);
        if (activeUser is null)
        {
            return null;
        }

        var emailColumnExists = await DoesColumnExistAsync(connection, "view_login", "Email", cancellationToken);

        var sql = $"""
            SELECT TOP 1
                UsuarioID,
                Login,
                Nome,
                FlagAdmin,
                {(emailColumnExists ? "Email" : "CAST(NULL AS NVARCHAR(256)) AS Email")},
                (SELECT TOP 1 U.Foto FROM BR_Usuario U WITH (NOLOCK) WHERE U.UsuarioID = view_login.UsuarioID) AS Foto,
                EstabelecimentoID,
                NmEstabelecimento,
                ApelidoEstabelecimento
            FROM view_login
            WHERE UsuarioID = @usuarioId;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", activeUser.Value.UsuarioId);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var user = MapViewLoginRow(reader);
        user.Email ??= activeUser.Value.Email;
        return user;
    }

    private static async Task<(int UsuarioId, string Email)?> GetActiveUserByEmailAsync(SqlConnection connection, string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 Email, UsuarioID
            FROM BR_Usuario WITH (NOLOCK)
            WHERE LOWER(Email) = LOWER(@email)
              AND FlagAtivo = 1;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var usuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioID"));
        var emailValue = reader.GetString(reader.GetOrdinal("Email"));
        return (usuarioId, emailValue);
    }

    private static async Task<string?> GetActiveEmailByUsuarioIdAsync(SqlConnection connection, int usuarioId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 Email
            FROM BR_Usuario WITH (NOLOCK)
            WHERE UsuarioID = @usuarioId
              AND FlagAtivo = 1
              AND Email IS NOT NULL;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result as string;
    }

    private static async Task<string?> CreateSessionAsync(SqlConnection connection, int usuarioId, string remoteIp, string? userAgent, CancellationToken cancellationToken)
    {
        await CleanupExpiredSessionsAsync(connection, usuarioId, cancellationToken);

        const string deactivateActiveSql = """
            UPDATE BR_UsuarioSessao
            SET Ativa = 0,
                ExpiraUtc = GETUTCDATE()
            WHERE UsuarioID = @usuarioId
              AND Ativa = 1;
            """;

        await using (var deactivateActiveCmd = new SqlCommand(deactivateActiveSql, connection))
        {
            deactivateActiveCmd.Parameters.AddWithValue("@usuarioId", usuarioId);
            await deactivateActiveCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        var token = Guid.NewGuid().ToString("N");
        const string insertSql = """
            INSERT INTO BR_UsuarioSessao (
                UsuarioID,
                SessaoToken,
                Ativa,
                InicioUtc,
                UltimaAtividadeUtc,
                ExpiraUtc,
                Ip,
                UserAgent
            )
            VALUES (
                @usuarioId,
                @sessionToken,
                1,
                GETUTCDATE(),
                GETUTCDATE(),
                DATEADD(MINUTE, @timeoutMinutes, GETUTCDATE()),
                @ip,
                @userAgent
            );
            """;

        await using var insertCmd = new SqlCommand(insertSql, connection);
        insertCmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        insertCmd.Parameters.AddWithValue("@sessionToken", token);
        insertCmd.Parameters.AddWithValue("@timeoutMinutes", SessionTimeoutMinutes);
        insertCmd.Parameters.AddWithValue("@ip", remoteIp);
        insertCmd.Parameters.AddWithValue("@userAgent", string.IsNullOrWhiteSpace(userAgent) ? DBNull.Value : userAgent);

        await insertCmd.ExecuteNonQueryAsync(cancellationToken);
        return token;
    }

    private static async Task<bool> RefreshSessionAsync(SqlConnection connection, int usuarioId, string sessionToken, string remoteIp, string? userAgent, CancellationToken cancellationToken)
    {
        await CleanupExpiredSessionsAsync(connection, usuarioId, cancellationToken);

        const string updateSql = """
            UPDATE BR_UsuarioSessao
            SET UltimaAtividadeUtc = GETUTCDATE(),
                ExpiraUtc = DATEADD(MINUTE, @timeoutMinutes, GETUTCDATE()),
                Ip = @ip,
                UserAgent = @userAgent
            WHERE UsuarioID = @usuarioId
              AND SessaoToken = @sessionToken
              AND Ativa = 1
              AND ExpiraUtc > GETUTCDATE();
            """;

        await using var cmd = new SqlCommand(updateSql, connection);
        cmd.Parameters.AddWithValue("@timeoutMinutes", SessionTimeoutMinutes);
        cmd.Parameters.AddWithValue("@ip", remoteIp);
        cmd.Parameters.AddWithValue("@userAgent", string.IsNullOrWhiteSpace(userAgent) ? DBNull.Value : userAgent);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        cmd.Parameters.AddWithValue("@sessionToken", sessionToken);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    private static async Task<bool> DeactivateSessionAsync(SqlConnection connection, int usuarioId, string sessionToken, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE BR_UsuarioSessao
            SET Ativa = 0,
                ExpiraUtc = GETUTCDATE()
            WHERE UsuarioID = @usuarioId
              AND SessaoToken = @sessionToken
              AND Ativa = 1;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        cmd.Parameters.AddWithValue("@sessionToken", sessionToken);
        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    private static async Task CleanupExpiredSessionsAsync(SqlConnection connection, int usuarioId, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE BR_UsuarioSessao
            SET Ativa = 0
            WHERE UsuarioID = @usuarioId
              AND Ativa = 1
              AND ExpiraUtc <= GETUTCDATE();
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private string BuildResetLink(string token)
    {
        var separator = _resetLinkBaseUrl.Contains('?') ? "&" : "?";
        return $"{_resetLinkBaseUrl}{separator}token={Uri.EscapeDataString(token)}";
    }

    private async Task<bool> IsAuthorizedForEstablishmentAsync(int usuarioId, bool isAdmin, int estabelecimentoId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = isAdmin
            ? """
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM BR_Estabelecimento E WITH (NOLOCK)
                    WHERE E.EstabelecimentoID = @estabelecimentoId
                      AND E.FlagAtivo = 1
                      AND E.SerieNF > 0
                      AND E.OrdemExibicao > 0
                ) THEN 1 ELSE 0 END;
                """
            : """
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM BR_UsuarioEstabelecimento X WITH (NOLOCK)
                    WHERE X.UsuarioID = @usuarioId
                      AND X.EstabelecimentoID = @estabelecimentoId
                ) THEN 1 ELSE 0 END;
                """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@estabelecimentoId", estabelecimentoId);
        if (!isAdmin)
        {
            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        }

        var result = (int)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);
        return result == 1;
    }

    private static async Task<bool> DoesColumnExistAsync(SqlConnection connection, string objectName, string columnName, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM sys.columns
                WHERE object_id = OBJECT_ID(@objectName)
                  AND name = @columnName
            ) THEN 1 ELSE 0 END;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@objectName", objectName);
        cmd.Parameters.AddWithValue("@columnName", columnName);

        var result = (int)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);
        return result == 1;
    }

    private async Task InsertLogAsync(SqlConnection connection, string message, int usuarioId, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO BrWeb..Intranet_Log (Modificacao, DataHora, UsuarioID)
            VALUES (@modificacao, GETDATE(), @usuarioId);
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@modificacao", message);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpdateLastLoginAsync(SqlConnection connection, int usuarioId, CancellationToken cancellationToken)
    {
        if (IgnoredUserIdsForLastLoginUpdate.Contains(usuarioId))
        {
            return;
        }

        const string sql = """
            UPDATE BrWeb..Permissoes_Intranet
            SET DtHrUltLogin = GETDATE()
            WHERE UsuarioID = @usuarioId;
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static ViewLoginRow MapViewLoginRow(SqlDataReader reader)
        => new()
        {
            UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioID")),
            Login = reader.GetString(reader.GetOrdinal("Login")),
            Nome = reader.GetString(reader.GetOrdinal("Nome")),
            FlagAdmin = ReadFlexibleBoolean(reader, "FlagAdmin"),
            Email = ReadNullableString(reader, "Email"),
            Foto = ReadNullableString(reader, "Foto"),
            EstabelecimentoId = ReadNullableInt32(reader, "EstabelecimentoID"),
            NmEstabelecimento = ReadNullableString(reader, "NmEstabelecimento"),
            ApelidoEstabelecimento = ReadNullableString(reader, "ApelidoEstabelecimento")
        };

    private static string? ReadNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? ReadNullableInt32(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static bool ReadFlexibleBoolean(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal))
        {
            return false;
        }

        var rawValue = reader.GetValue(ordinal);

        return rawValue switch
        {
            bool boolValue => boolValue,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            string stringValue when int.TryParse(stringValue, out var numericValue) => numericValue != 0,
            string stringValue when bool.TryParse(stringValue, out var booleanValue) => booleanValue,
            _ => false
        };
    }

    private sealed class ViewLoginRow
    {
        public int UsuarioId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public bool FlagAdmin { get; set; }
        public string? Email { get; set; }
        public string? Foto { get; set; }
        public int? EstabelecimentoId { get; set; }
        public string? NmEstabelecimento { get; set; }
        public string? ApelidoEstabelecimento { get; set; }
    }

    private sealed class PasswordResetTokenInfo
    {
        public int UsuarioId { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
