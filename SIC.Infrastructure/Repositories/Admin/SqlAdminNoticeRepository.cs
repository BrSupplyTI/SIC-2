using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.Admin;
using SIC.Domain.Entities.Admin;
using System.Data;

namespace SIC.Infrastructure.Repositories.Admin;

public sealed class SqlAdminNoticeRepository(IConfiguration configuration) : IAdminNoticeRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<AdminNotice>> GetAllNoticesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT A.AvisoID,
                   A.Titulo,
                   A.Descricao,
                   A.Prioridade,
                   A.DataHoraEnvio,
                   A.DataHoraExpiracao,
                   R.NmUsuario AS Responsavel,
                   CASE WHEN ISNULL(A.UsuarioID,0) = 0 AND ISNULL(A.IntranetAreaID,0) = 0
                        THEN 'Todos os Usuários'
                        ELSE CASE WHEN ISNULL(A.IntranetAreaID,0) > 0
                                  THEN I.NmArea
                                  ELSE U.NmUsuario
                             END
                   END AS Destinatario,
                   CASE WHEN A.DataHoraExpiracao < GETDATE()
                        THEN 'Expirada'
                        ELSE 'Ativa'
                   END AS Situacao,
                   (SELECT COUNT(*)
                    FROM BR_AvisoUsuario AU WITH (NOLOCK)
                    WHERE AU.AvisoID = A.AvisoID) AS QtLeituras
            FROM BR_Aviso A WITH (NOLOCK)
            JOIN BR_Usuario R WITH (NOLOCK) ON R.UsuarioID = A.UsuarioResponsavelID
            LEFT JOIN BrWeb..Intranet_Area I WITH (NOLOCK) ON I.IntranetAreaID = A.IntranetAreaID
            LEFT JOIN BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = A.UsuarioID
            WHERE A.DataHoraEnvio >= GETDATE() - 365
            ORDER BY A.DataHoraExpiracao DESC
            """;

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<AdminNotice>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AdminNotice
            {
                AvisoID = reader.GetInt32(reader.GetOrdinal("AvisoID")),
                Titulo = ReadString(reader, "Titulo"),
                Descricao = ReadString(reader, "Descricao"),
                Prioridade = reader.GetInt32(reader.GetOrdinal("Prioridade")),
                DataHoraEnvio = reader.GetDateTime(reader.GetOrdinal("DataHoraEnvio")),
                DataHoraExpiracao = reader.GetDateTime(reader.GetOrdinal("DataHoraExpiracao")),
                Responsavel = ReadString(reader, "Responsavel"),
                Destinatario = ReadString(reader, "Destinatario"),
                Situacao = ReadString(reader, "Situacao"),
                QtLeituras = reader.GetInt32(reader.GetOrdinal("QtLeituras"))
            });
        }

        return items;
    }

    public async Task ExpireNoticeAsync(int avisoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "UPDATE BR_Aviso SET DataHoraExpiracao = GETDATE() WHERE AvisoID = @AvisoID";

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@AvisoID", SqlDbType.Int).Value = avisoId;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteNoticeAsync(int avisoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "DELETE FROM BR_Aviso WHERE AvisoID = @AvisoID";

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@AvisoID", SqlDbType.Int).Value = avisoId;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();
    }

    public async Task CreateNoticeAsync(string titulo, string descricao, int prioridade, DateTime dataHoraExpiracao, int? intranetAreaId, int? usuarioId, int usuarioResponsavelId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO BR_Aviso (
                Titulo,
                Descricao,
                Prioridade,
                DataHoraEnvio,
                DataHoraExpiracao,
                IntranetAreaID,
                UsuarioID,
                UsuarioResponsavelID
            ) VALUES (
                @Titulo,
                @Descricao,
                @Prioridade,
                GETDATE(),
                @DataHoraExpiracao,
                @IntranetAreaID,
                @UsuarioID,
                @UsuarioResponsavelID
            )
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Titulo", SqlDbType.VarChar, 100).Value = titulo;
        cmd.Parameters.Add("@Descricao", SqlDbType.VarChar, 4000).Value = descricao;
        cmd.Parameters.Add("@Prioridade", SqlDbType.Int).Value = prioridade;
        cmd.Parameters.Add("@DataHoraExpiracao", SqlDbType.DateTime).Value = dataHoraExpiracao;
        cmd.Parameters.Add("@IntranetAreaID", SqlDbType.Int).Value = intranetAreaId.HasValue ? intranetAreaId.Value : DBNull.Value;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId.HasValue ? usuarioId.Value : DBNull.Value;
        cmd.Parameters.Add("@UsuarioResponsavelID", SqlDbType.Int).Value = usuarioResponsavelId;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IntranetArea>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT IntranetAreaID,
                   NmArea
            FROM BrWeb..Intranet_Area WITH (NOLOCK)
            ORDER BY NmArea
            """;

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<IntranetArea>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new IntranetArea
            {
                IntranetAreaID = reader.GetInt32(reader.GetOrdinal("IntranetAreaID")),
                NmArea = ReadString(reader, "NmArea")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<AdminUser>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT UsuarioID,
                   NmUsuario
            FROM BR_Usuario WITH (NOLOCK)
            WHERE FlagAtivo = 1
            ORDER BY NmUsuario
            """;

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<AdminUser>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AdminUser
            {
                UsuarioID = reader.GetInt32(reader.GetOrdinal("UsuarioID")),
                NmUsuario = ReadString(reader, "NmUsuario")
            });
        }

        return items;
    }
}
