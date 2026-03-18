using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlAuthRepository(IConfiguration configuration) : IAuthRepository
{
    private static readonly HashSet<int> IgnoredUserIdsForLastLoginUpdate = [685, 1671];
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<AuthUser?> GetUserByPasswordAsync(string login, string password, string? masterPassword, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

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
        cmd.Parameters.AddWithValue("@masterPassword", (object?)masterPassword ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapAuthUser(reader);
    }

    public async Task<AuthUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

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

        var user = MapAuthUser(reader);
        user.Email ??= activeUser.Value.Email;
        return user;
    }

    public async Task<AuthUser?> GetUserByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

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

        return MapAuthUser(reader);
    }

    public async Task<string?> GetActiveEmailByUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 Email
            FROM BR_Usuario WITH (NOLOCK)
            WHERE UsuarioID = @usuarioId
              AND FlagAtivo = 1
              AND Email IS NOT NULL;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result as string;
    }

    public async Task<bool> TryCreateSessionAsync(int usuarioId, string sessionToken, int timeoutMinutes, string remoteIp, string? userAgent, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await CleanupExpiredSessionsInternalAsync(connection, usuarioId, cancellationToken);

        const string deactivateActiveSql = """
            UPDATE BR_UsuarioSessao
            SET Ativa = 0,
                ExpiraUtc = GETUTCDATE()
            WHERE UsuarioID = @usuarioId
              AND Ativa = 1;
            """;

        await using (var deactivateCmd = new SqlCommand(deactivateActiveSql, connection))
        {
            deactivateCmd.Parameters.AddWithValue("@usuarioId", usuarioId);
            await deactivateCmd.ExecuteNonQueryAsync(cancellationToken);
        }

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
        insertCmd.Parameters.AddWithValue("@sessionToken", sessionToken);
        insertCmd.Parameters.AddWithValue("@timeoutMinutes", timeoutMinutes);
        insertCmd.Parameters.AddWithValue("@ip", remoteIp);
        insertCmd.Parameters.AddWithValue("@userAgent", string.IsNullOrWhiteSpace(userAgent) ? DBNull.Value : userAgent);

        var rows = await insertCmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> RefreshSessionAsync(int usuarioId, string sessionToken, int timeoutMinutes, string remoteIp, string? userAgent, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await CleanupExpiredSessionsInternalAsync(connection, usuarioId, cancellationToken);

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
        cmd.Parameters.AddWithValue("@timeoutMinutes", timeoutMinutes);
        cmd.Parameters.AddWithValue("@ip", remoteIp);
        cmd.Parameters.AddWithValue("@userAgent", string.IsNullOrWhiteSpace(userAgent) ? DBNull.Value : userAgent);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        cmd.Parameters.AddWithValue("@sessionToken", sessionToken);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> DeactivateSessionAsync(int usuarioId, string sessionToken, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BR_UsuarioSessao
            SET Ativa = 0,
                ExpiraUtc = GETUTCDATE()
            WHERE UsuarioID = @usuarioId
              AND SessaoToken = @sessionToken
              AND Ativa = 1;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        cmd.Parameters.AddWithValue("@sessionToken", sessionToken);
        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task CleanupExpiredSessionsAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await CleanupExpiredSessionsInternalAsync(connection, usuarioId, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthEstablishment>> GetAuthorizedEstablishmentsAsync(int usuarioId, bool isAdmin, CancellationToken cancellationToken = default)
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

        var result = new List<AuthEstablishment>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AuthEstablishment
            {
                EstabelecimentoId = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                NmEstabelecimento = reader.GetString(reader.GetOrdinal("NmEstabelecimento")),
                CdEstabelecimento = ReadNullableString(reader, "CdEstabelecimento")
            });
        }

        return result;
    }

    public async Task<bool> IsAuthorizedForEstablishmentAsync(int usuarioId, bool isAdmin, int estabelecimentoId, CancellationToken cancellationToken = default)
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

    public async Task<bool> ChangeEstablishmentAsync(int usuarioId, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BR_Usuario
            SET EstabelecimentoID = @estabelecimentoId
            WHERE UsuarioID = @usuarioId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@estabelecimentoId", estabelecimentoId);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> UpdateUserPhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BR_Usuario
            SET Foto = @foto
            WHERE UsuarioID = @usuarioId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@foto", foto);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> ChangePasswordAsync(int usuarioId, string newPassword, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BR_Usuario
            SET Senha = @senha
            WHERE UsuarioID = @usuarioId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@senha", newPassword);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> ResetPasswordAsync(int usuarioId, string newPassword, string? updatePasswordSql, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(updatePasswordSql))
        {
            return false;
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(updatePasswordSql, connection);
        cmd.Parameters.AddWithValue("@novaSenha", newPassword);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task UpdateLastLoginAsync(int usuarioId, CancellationToken cancellationToken = default)
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertLogAsync(string message, int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO BrWeb..Intranet_Log (Modificacao, DataHora, UsuarioID)
            VALUES (@modificacao, GETDATE(), @usuarioId);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@modificacao", message);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
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

    private static async Task CleanupExpiredSessionsInternalAsync(SqlConnection connection, int usuarioId, CancellationToken cancellationToken)
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

    private static AuthUser MapAuthUser(SqlDataReader reader)
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

    private static DateTime? ReadNullableDateTime(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
