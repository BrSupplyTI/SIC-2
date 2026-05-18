using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.Cotacao;

namespace SIC.Infrastructure.Repositories.Cotacao;

/// <summary>
/// Implementação SQL das operações de escrita da Cotação.
/// </summary>
public sealed class SqlCotacaoCommandRepository(IConfiguration configuration) : ICotacaoCommandRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public Task<(bool Success, string? Error)> AdicionarItemAsync(
        int propostaId,
        string codItemBR,
        string descrItemBR,
        string tipoCusto,
        decimal precoItem,
        decimal vlrCustoAquisicao,
        decimal vlrCustoMedio,
        int quantidade,
        decimal vlrPrecoMinimo,
        decimal vlrTabelaPreco,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            INSERT INTO BrWeb.dbo.Proposta_Itens
                (PropostaID, CodItemBR, DescrItemBR, TipoCusto, PrecoItem, VlrCustoAquisicao, VlrCustoMedio, Quantidade, VlrPrecoMinimo, VlrTabelaPreco, Status)
            VALUES
                (@PropostaID, @CodItemBR, @DescrItemBR, @TipoCusto, @PrecoItem, @VlrCustoAquisicao, @VlrCustoMedio, @Quantidade, @VlrPrecoMinimo, @VlrTabelaPreco, 0);
            """,
            parameters =>
            {
                parameters.AddWithValue("@PropostaID", propostaId);
                parameters.AddWithValue("@CodItemBR", codItemBR);
                parameters.AddWithValue("@DescrItemBR", descrItemBR);
                parameters.AddWithValue("@TipoCusto", tipoCusto);
                parameters.AddWithValue("@PrecoItem", precoItem);
                parameters.AddWithValue("@VlrCustoAquisicao", vlrCustoAquisicao);
                parameters.AddWithValue("@VlrCustoMedio", vlrCustoMedio);
                parameters.AddWithValue("@Quantidade", quantidade);
                parameters.AddWithValue("@VlrPrecoMinimo", vlrPrecoMinimo);
                parameters.AddWithValue("@VlrTabelaPreco", vlrTabelaPreco);
            },
            cancellationToken);

    public async Task<(bool Success, string? Error)> CalcularMargemItemAsync(
        int propostaId,
        int propostaItemId,
        string type,
        string viaTela,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var cmd = new SqlCommand(
                """
                EXEC Integracao_Clientes..BR_SAP_CotacaoPropostaPorItem
                    @PropostaID     = @PropostaID,
                    @PropostaItemID = @PropostaItemID,
                    @Type           = @Type,
                    @ViaTela        = @ViaTela
                """, connection);

            cmd.CommandTimeout = 600; // 10 minutos — igual ao timeout configurado na procedure

            cmd.Parameters.AddWithValue("@PropostaID", propostaId);
            cmd.Parameters.AddWithValue("@PropostaItemID",
                propostaItemId == 0 ? (object)DBNull.Value : propostaItemId);
            cmd.Parameters.AddWithValue("@Type",    type);
            cmd.Parameters.AddWithValue("@ViaTela", viaTela);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Task<(bool Success, string? Error)> AtualizarItemAsync(
        int propostaId,
        int propostaItemId,
        decimal precoUnitario,
        decimal quantidade,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE BrWeb.dbo.Proposta_Itens
            SET PrecoItem    = @PrecoItem,
                Quantidade   = @Quantidade,
                VlrPrecoVenda = 0,
                ValorICMS    = 0,
                IPI          = 0,
                ST           = 0,
                Margem       = 0,
                Percentual   = 0,
                ValorLiqUnit = 0
            WHERE PropostaID = @PropostaID
              AND PropostaItemID = @PropostaItemID
            """,
            parameters =>
            {
                parameters.AddWithValue("@PropostaID",     propostaId);
                parameters.AddWithValue("@PropostaItemID", propostaItemId);
                parameters.AddWithValue("@PrecoItem",      precoUnitario);
                parameters.AddWithValue("@Quantidade",     quantidade);
            },
            cancellationToken);

    public Task<(bool Success, string? Error)> AtualizarCustoItemAsync(
        int propostaId,
        int propostaItemId,
        string tipoCusto,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE BrWeb.dbo.Proposta_Itens
            SET TipoCusto = @TipoCusto
            WHERE PropostaID     = @PropostaID
              AND PropostaItemID = @PropostaItemID
            """,
            parameters =>
            {
                parameters.AddWithValue("@PropostaID",     propostaId);
                parameters.AddWithValue("@PropostaItemID", propostaItemId);
                parameters.AddWithValue("@TipoCusto",      tipoCusto);
            },
            cancellationToken);

    public Task<(bool Success, string? Error)> GerarItensAsync(
        int propostaId,
        string tipoGeracao,
        int usuarioId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            "EXEC BrWeb..Proposta_GerarItens @PropostaID, @TipoGeracao, @UsuarioID",
            parameters =>
            {
                parameters.AddWithValue("@PropostaID",  propostaId);
                parameters.AddWithValue("@TipoGeracao", tipoGeracao);
                parameters.AddWithValue("@UsuarioID",   usuarioId);
            },
            cancellationToken);

    public async Task<(bool Success, string? Error)> RemoverItensAsync(
        int propostaId,
        IReadOnlyList<(int PropostaItemId, string CdItem)> itens,
        string motivo,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                // DELETE em lote usando parâmetros dinâmicos
                var paramNames = itens.Select((_, i) => $"@id{i}");
                var deleteSql = $"DELETE FROM BrWeb.dbo.Proposta_Itens WHERE PropostaItemID IN ({string.Join(", ", paramNames)})";
                await using var deleteCmd = new SqlCommand(deleteSql, connection, (SqlTransaction)transaction);
                for (var i = 0; i < itens.Count; i++)
                    deleteCmd.Parameters.AddWithValue($"@id{i}", itens[i].PropostaItemId);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken);

                // LOG: um registro por item removido
                const string logSql = """
                    INSERT INTO BrWeb.dbo.Proposta_Log(PropostaID, CdItem, Motivo, Data, UsuarioID)
                    VALUES (@PropostaID, @CdItem, @Motivo, GETDATE(), @UsuarioID)
                    """;
                foreach (var (_, cdItem) in itens)
                {
                    await using var logCmd = new SqlCommand(logSql, connection, (SqlTransaction)transaction);
                    logCmd.Parameters.AddWithValue("@PropostaID", propostaId);
                    logCmd.Parameters.AddWithValue("@CdItem",     cdItem);
                    logCmd.Parameters.AddWithValue("@Motivo",     motivo);
                    logCmd.Parameters.AddWithValue("@UsuarioID",  usuarioId);
                    await logCmd.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Task<(bool Success, string? Error)> SalvarCondPagtoAsync(
        int propostaId,
        int condPagtoId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE BrWeb.dbo.Proposta
            SET CondPagto = @CondPagto
            WHERE PropostaId = @PropostaID
            """,
            parameters =>
            {
                parameters.AddWithValue("@PropostaID", propostaId);
                parameters.AddWithValue("@CondPagto",  condPagtoId);
            },
            cancellationToken);

    public Task<(bool Success, string? Error)> RecalcularMargemBrutaPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE BrWeb.dbo.Proposta
            SET MargemBruta = (
                SELECT
                    CASE
                        WHEN SUM(I.VlrPrecoVenda) = 0 THEN 0
                        ELSE (SUM(I.VlrContribuido) / SUM(I.VlrPrecoVenda)) * 100
                    END
                FROM BrWeb.dbo.Proposta_Itens I WITH (NOLOCK)
                WHERE I.PropostaID = @PropostaID
                  AND I.VlrPrecoVenda > 0
            )
            WHERE PropostaId = @PropostaID
            """,
            parameters =>
            {
                parameters.AddWithValue("@PropostaID", propostaId);
            },
            cancellationToken);

    private async Task<(bool Success, string? Error)> ExecuteAsync(
        string sql,
        Action<SqlParameterCollection> addParameters,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var cmd = new SqlCommand(sql, connection);
            addParameters(cmd.Parameters);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
