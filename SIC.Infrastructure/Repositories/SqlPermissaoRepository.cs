using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using System.Data;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlPermissaoRepository(IConfiguration configuration) : IPermissaoRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<bool> TemPermissaoAsync(int usuarioId, int permissaoId, CancellationToken cancellationToken = default)
    {
        if (usuarioId <= 0 || permissaoId <= 0)
            return false;

        const string sql = @"
            SELECT COUNT(*) AS TemPermissao
            FROM BrWeb..Intranet_PermissoesUsuario (NOLOCK)
            WHERE UsuarioID = @UsuarioID
              AND PermissaoID = @PermissaoID;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
        cmd.Parameters.AddWithValue("@PermissaoID", permissaoId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var count = result is null || result is DBNull ? 0 : Convert.ToInt32(result);
        return count > 0;
    }
}
