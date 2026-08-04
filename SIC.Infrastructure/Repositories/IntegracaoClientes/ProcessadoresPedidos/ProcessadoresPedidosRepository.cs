using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.IntegracaoClientes.ProcessadoresPedidos;
using SIC.Domain.Entities.IntegracaoClientes.ProcessadoresPedidos;

namespace SIC.Infrastructure.Repositories.IntegracaoClientes.ProcessadoresPedidos
{
    public sealed class ProcessadoresPedidosRepository(IConfiguration configuration) : IProcessadorPedidoRepository
    {
        private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

        public async Task<List<ProcessadorPedido>> GetAllAsync(CancellationToken cancellationToken)
        {
            var processadores = new List<ProcessadorPedido>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("SELECT ProcessadorPedidoId, Nome FROM Integracao_Clientes.dbo.PPedido_ProcessadorPedido", connection);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                processadores.Add(new ProcessadorPedido
                {
                    ProcessadorPedidoId = reader.GetInt32(0),
                    Nome = reader.GetString(1)
                });
            }

            return processadores;
        }

        public async Task<List<ProcessadorPedidoConfiguracao>> GetConfiguracoesAsync(int processadorPedidoId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    ppc.*,
                    bc.CdExtCliente AS CodigoCliente,
                    bc.RazaoSocialCliente
                FROM Integracao_Clientes.dbo.PPedido_ProcessadorPedidoConfiguracao AS ppc
                INNER JOIN BrSupply.dbo.BR_Cliente AS bc WITH (NOLOCK)
                    ON bc.ClienteID = ppc.ClienteID
                WHERE ppc.ProcessadorPedidoId = @ProcessadorPedidoId;
                """;

            var configuracoes = new List<ProcessadorPedidoConfiguracao>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ProcessadorPedidoId", processadorPedidoId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var codigoCliente = GetString(reader, "CodigoCliente");
                var deParaCliente = GetOptionalString(reader, "CdExtCliente", "DeParaCliente", "CdExtClienteCliente");

                configuracoes.Add(new ProcessadorPedidoConfiguracao
                {
                    ProcessadorPedidoId = GetInt32(reader, "ProcessadorPedidoId"),
                    ClienteId = GetInt32(reader, "ClienteID"),
                    CodigoCliente = codigoCliente,
                    RazaoSocialCliente = GetString(reader, "RazaoSocialCliente"),
                    DeParaCliente = string.IsNullOrWhiteSpace(deParaCliente) ? codigoCliente : deParaCliente
                });
            }

            return configuracoes;
        }

        private static int GetInt32(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static string GetString(SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetValue(ordinal).ToString() ?? string.Empty;
        }

        private static string GetOptionalString(SqlDataReader reader, params string[] candidateColumns)
        {
            foreach (var columnName in candidateColumns)
            {
                if (!HasColumn(reader, columnName))
                {
                    continue;
                }

                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? string.Empty : reader.GetValue(ordinal).ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
