using Microsoft.Data.SqlClient;
using SIC.Api.Domain.Entities;

namespace SIC.Api.Repositories;

public sealed class SqlUserProfileRepository(IConfiguration configuration) : IUserProfileRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<AreaOption>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT IntranetAreaID, NmArea
            FROM BrWeb..Intranet_Area WITH (NOLOCK)
            WHERE FlagAtivo = 1
            ORDER BY NmArea ASC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        var result = new List<AreaOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AreaOption
            {
                AreaId = reader.GetInt32(reader.GetOrdinal("IntranetAreaID")),
                Nome = reader.GetString(reader.GetOrdinal("NmArea"))
            });
        }

        return result;
    }

    public async Task<UserProfile?> GetProfileAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1
                U.UsuarioID,
                U.NmUsuario,
                U.Email,
                U.Telefone,
                U.Ramal,
                U.Matricula,
                U.Cargo,
                U.Setor,
                U.Foto,
                I.IntranetAreaID,
                A.NmArea,
                I.FlagAdmin,
                I.FlagBackOffice,
                I.FlagAlteraEstabelecimento
            FROM BR_Usuario U WITH (NOLOCK)
            INNER JOIN BrWeb..Permissoes_Intranet I WITH (NOLOCK) ON I.UsuarioID = U.UsuarioID
            LEFT JOIN BrWeb..Intranet_Area A WITH (NOLOCK) ON A.IntranetAreaID = I.IntranetAreaID
            WHERE U.UsuarioID = @usuarioId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserProfile
        {
            UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioID")),
            Nome = reader.GetString(reader.GetOrdinal("NmUsuario")),
            Email = ReadNullableString(reader, "Email"),
            Telefone = ReadNullableString(reader, "Telefone"),
            Ramal = ReadNullableString(reader, "Ramal"),
            Matricula = ReadNullableInt(reader, "Matricula"),
            Cargo = ReadNullableString(reader, "Cargo"),
            Setor = ReadNullableString(reader, "Setor"),
            Foto = ReadNullableString(reader, "Foto"),
            AreaId = ReadNullableInt(reader, "IntranetAreaID"),
            AreaNome = ReadNullableString(reader, "NmArea"),
            FlagAdmin = ReadFlexibleBoolean(reader, "FlagAdmin"),
            FlagBackOffice = ReadFlexibleBoolean(reader, "FlagBackOffice"),
            FlagAlteraEstabelecimento = ReadFlexibleBoolean(reader, "FlagAlteraEstabelecimento")
        };
    }

    public async Task<IReadOnlyList<UserPermission>> GetPermissionsAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                P.Modulo,
                P.NmPermissao,
                U.ConcedidoPor,
                U.DataHora
            FROM BrWeb..Intranet_PermissoesUsuario U WITH (NOLOCK)
            INNER JOIN BrWeb..Intranet_Permissoes P WITH (NOLOCK) ON P.PermissaoID = U.PermissaoID
            WHERE U.UsuarioID = @usuarioId
            ORDER BY P.Modulo, P.NmPermissao;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var permissions = new List<UserPermission>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            permissions.Add(new UserPermission
            {
                Modulo = ReadNullableString(reader, "Modulo") ?? string.Empty,
                NomePermissao = ReadNullableString(reader, "NmPermissao") ?? string.Empty,
                ConcedidoPor = ReadNullableString(reader, "ConcedidoPor"),
                DataHora = ReadNullableDateTime(reader, "DataHora")
            });
        }

        return permissions;
    }

    public async Task<bool> UpdateProfileAsync(int usuarioId, int? areaId, string? telefone, string? ramal, int? matricula, string? cargo, string? setor, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BR_Usuario
            SET Telefone = @telefone,
                Ramal = @ramal,
                Matricula = @matricula,
                Cargo = @cargo,
                Setor = @setor
            WHERE UsuarioID = @usuarioId;

            UPDATE BrWeb..Permissoes_Intranet 
            SET IntranetAreaID = @areaId
            WHERE UsuarioID = @usuarioId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        cmd.Parameters.AddWithValue("@telefone", string.IsNullOrWhiteSpace(telefone) ? DBNull.Value : telefone);
        cmd.Parameters.AddWithValue("@ramal", string.IsNullOrWhiteSpace(ramal) ? DBNull.Value : ramal);
        cmd.Parameters.AddWithValue("@matricula", matricula.HasValue ? matricula.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@cargo", string.IsNullOrWhiteSpace(cargo) ? DBNull.Value : cargo);
        cmd.Parameters.AddWithValue("@setor", string.IsNullOrWhiteSpace(setor) ? DBNull.Value : setor);
        cmd.Parameters.AddWithValue("@areaId", areaId.HasValue ? areaId.Value : DBNull.Value);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> UpdatePhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BR_Usuario
            SET Foto = @foto
            WHERE UsuarioID = @usuarioId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        cmd.Parameters.AddWithValue("@foto", foto);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> RemovePhotoAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BR_Usuario
            SET Foto = NULL
            WHERE UsuarioID = @usuarioId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    private static string? ReadNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? ReadNullableInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static DateTime? ReadNullableDateTime(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static bool ReadFlexibleBoolean(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal))
        {
            return false;
        }

        var value = reader.GetValue(ordinal);
        return value switch
        {
            bool boolValue => boolValue,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            string str when int.TryParse(str, out var parsed) => parsed != 0,
            _ => false
        };
    }
}
