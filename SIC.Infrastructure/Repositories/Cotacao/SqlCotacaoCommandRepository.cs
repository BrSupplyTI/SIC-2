using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.Cotacao;
using SIC.Domain.Entities.Cotacao;

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

    // ── Finalizar ─────────────────────────────────────────────────────────────

    public async Task<(bool Success, int? StatusId, string? Error)> FinalizarAsync(
        int propostaId,
        string dataValidade,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var statusId = 2;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string atendenteSql = """
                SELECT ISNULL(U.FlagPrecisaAprovacao, 0) AS FlagPrecisaAprovacao,
                       ISNULL(U.PercMargemMinPedido, 0)  AS PercMargemMinPedido,
                       ISNULL(U.PercMargemMaxPedido, 0)  AS PercMargemMaxPedido
                FROM BrSupply.dbo.BR_Usuario U (NOLOCK)
                WHERE U.UsuarioID = @UsuarioID
                """;

            await using (var cmdAtendente = new SqlCommand(atendenteSql, connection))
            {
                cmdAtendente.Parameters.AddWithValue("@UsuarioID", usuarioId);
                await using var readerAtendente = await cmdAtendente.ExecuteReaderAsync(cancellationToken);

                if (await readerAtendente.ReadAsync(cancellationToken))
                {
                    var flagPrecisaAprovacao = readerAtendente.GetInt32(readerAtendente.GetOrdinal("FlagPrecisaAprovacao")) == 1;

                    if (flagPrecisaAprovacao)
                    {
                        var minMargem = ReadDecimal(readerAtendente, "PercMargemMinPedido");
                        var maxMargem = ReadDecimal(readerAtendente, "PercMargemMaxPedido");
                        await readerAtendente.CloseAsync();

                        const string margemSql = """
                            SELECT ISNULL(MargemBruta, 0) AS MargemBruta
                            FROM BrWeb.dbo.Proposta (NOLOCK)
                            WHERE PropostaId = @PropostaID
                            """;

                        await using var cmdMargem = new SqlCommand(margemSql, connection);
                        cmdMargem.Parameters.AddWithValue("@PropostaID", propostaId);
                        await using var readerMargem = await cmdMargem.ExecuteReaderAsync(cancellationToken);

                        if (await readerMargem.ReadAsync(cancellationToken))
                        {
                            var margem = ReadDecimal(readerMargem, "MargemBruta");
                            if (margem < minMargem || margem > maxMargem)
                                statusId = 10;
                        }
                    }
                }
            }

            const string updateSql = """
                UPDATE BrWeb.dbo.Proposta
                SET DataValidade = @DataValidade,
                    StatusID     = @StatusID,
                    UsuarioID    = @UsuarioID
                WHERE PropostaId = @PropostaID
                """;

            await using var cmdUpdate = new SqlCommand(updateSql, connection);
            cmdUpdate.Parameters.AddWithValue("@DataValidade", dataValidade);
            cmdUpdate.Parameters.AddWithValue("@StatusID",     statusId);
            cmdUpdate.Parameters.AddWithValue("@UsuarioID",    usuarioId);
            cmdUpdate.Parameters.AddWithValue("@PropostaID",   propostaId);

            var rows = await cmdUpdate.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0
                ? (true, statusId, null)
                : (false, null, "Nenhuma linha afetada.");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    // ── Aprovar ───────────────────────────────────────────────────────────────

    public Task<(bool Success, string? Error)> AprovarAsync(
        int propostaId,
        int aprovadorId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE BrWeb.dbo.Proposta
            SET StatusID           = 2,
                AprovadorUsuarioID = @AprovadorID
            WHERE PropostaId = @PropostaID
            """,
            parameters =>
            {
                parameters.AddWithValue("@AprovadorID", aprovadorId);
                parameters.AddWithValue("@PropostaID",  propostaId);
            },
            cancellationToken);

    // ── Reprovar ──────────────────────────────────────────────────────────────

    public Task<(bool Success, string? Error)> ReprovarAsync(
        int propostaId,
        int aprovadorId,
        string justificativa,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE BrWeb.dbo.Proposta
            SET StatusID               = 1,
                AprovadorUsuarioID     = @AprovadorID,
                JustificativaAprovador = @Justificativa
            WHERE PropostaId = @PropostaID
            """,
            parameters =>
            {
                parameters.AddWithValue("@AprovadorID",  aprovadorId);
                parameters.AddWithValue("@Justificativa", justificativa);
                parameters.AddWithValue("@PropostaID",   propostaId);
            },
            cancellationToken);

    // ── SalvarFreteProposta ───────────────────────────────────────────────────

    public Task<(bool Success, string? Error)> SalvarFretePropostaAsync(
        int propostaId,
        int transportadoraId,
        decimal valorFrete,
        int prazoTotal,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE BrWeb.dbo.Proposta
            SET TransportadoraID = @TransportadoraID,
                Frete            = @Frete,
                DiasPrazoEntrega = @DiasPrazoEntrega
            WHERE PropostaId = @PropostaID
            """,
            parameters =>
            {
                parameters.AddWithValue("@TransportadoraID", transportadoraId);
                parameters.AddWithValue("@Frete",            valorFrete);
                parameters.AddWithValue("@DiasPrazoEntrega", prazoTotal);
                parameters.AddWithValue("@PropostaID",       propostaId);
            },
            cancellationToken);

    // ── AutorizarFaturamento ──────────────────────────────────────────────────

    public async Task<(bool Success, int? CotacaoId, string? Error)> AutorizarFaturamentoAsync(
        int propostaId,
        string ipAprovacao,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            const string sqlEncerraEnvios = """
                IF OBJECT_ID('BrWeb.dbo.PropostaCotacaoEnvio', 'U') IS NOT NULL
                BEGIN
                    UPDATE BrWeb.dbo.PropostaCotacaoEnvio
                    SET StatusID          = 3,
                        DataHoraAprovacao = GETDATE(),
                        IPAprovacao       = @IPAprovacao
                    WHERE PropostaID = @PropostaID
                END
                """;

            await using (var cmd = new SqlCommand(sqlEncerraEnvios, conn))
            {
                cmd.Parameters.AddWithValue("@IPAprovacao", ipAprovacao);
                cmd.Parameters.AddWithValue("@PropostaID",  propostaId);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            const string sqlGeraPedido = "EXEC BrSupply.dbo.BR_sp_Proposta_GeraPedido_LocalEntrega @PropostaID";
            await using (var cmd = new SqlCommand(sqlGeraPedido, conn))
            {
                cmd.Parameters.AddWithValue("@PropostaID", propostaId);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            const string sqlBuscaPedido = """
                SELECT CotacaoID
                FROM BrWeb.dbo.Proposta
                WHERE PropostaID = @PropostaID
                """;

            await using var cmdBusca = new SqlCommand(sqlBuscaPedido, conn);
            cmdBusca.Parameters.AddWithValue("@PropostaID", propostaId);

            var result = await cmdBusca.ExecuteScalarAsync(cancellationToken);
            var cotacaoId = result is DBNull or null ? (int?)null : Convert.ToInt32(result);
            return (true, cotacaoId, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    // ── Helper privado ────────────────────────────────────────────────────────

    private static decimal ReadDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return 0m;
        return reader.GetFieldType(ordinal) == typeof(decimal)
            ? reader.GetDecimal(ordinal)
            : Convert.ToDecimal(reader.GetValue(ordinal));
    }

    // ─── CotacaoAddService commands ───────────────────────────────────────────

    public async Task<int> CriarPropostaAsync(
        CriarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        const string insertSql = """
            INSERT INTO BrWeb.dbo.Proposta
            (
                Nome, TipoCotacao, TipoID, EstabelecimentoID, ClienteId, ClienteEnderecoID, ClienteLocalEntregaID,
                ObsLocalEntrega, TabelaPrecoID, FlagPrecoConformeTabela,
                UfOrigem, UfDestino, CodigoIBGE, MargemPadrao,
                DataValidade, CondPagto, FormaPagamentoSAP, tipoOVSAP, OrdemCompra, NrContrato,
                TipoMotivoIDSAP,
                ContatoNome, ContatoEmail, Obs,
                UsuarioId, DtCriacao, Versao, StatusID, NatOperacao,
                ValorVendaTotal, Frete, VlrPedidoMinimo
            )
            OUTPUT INSERTED.PropostaId
            VALUES
            (
                @Nome, @TipoCotacao, 2, @EstabelecimentoID, @ClienteId, @ClienteEnderecoID, @ClienteLocalEntregaID,
                @ObsLocalEntrega, @TabelaPrecoID, @FlagPrecoConformeTabela,
                @UfOrigem, @UfDestino, @CodigoIBGE, @MargemPadrao,
                @DataValidade, @CondPagto, @FormaPagamentoSAP, @TipoOVSAP, @OrdemCompra, @NrContrato,
                @TipoMotivoIDSAP,
                @ContatoNome, @ContatoEmail, @Obs,
                @UsuarioId, GETDATE(), 1, 1, 1,
                @ValorVendaTotal, @Frete, @VlrPedidoMinimo
            )
            """;

        const string updateCdProposta = """
            UPDATE BrWeb.dbo.Proposta SET CdProposta = @CdProposta WHERE PropostaId = @PropostaId
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var insertCmd = new SqlCommand(insertSql, connection);
        insertCmd.Parameters.AddWithValue("@Nome",                    (object?)request.Nome ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TipoCotacao",             (object?)request.TipoNome ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@EstabelecimentoID",       request.EstabelecimentoID);
        insertCmd.Parameters.AddWithValue("@ClienteId",               request.ClienteId);
        insertCmd.Parameters.AddWithValue("@ClienteEnderecoID",       (object?)request.ClienteEnderecoID ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ClienteLocalEntregaID",   (object?)request.ClienteLocalEntregaID ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ObsLocalEntrega",         (object?)request.ObsLocalEntrega ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TabelaPrecoID",           (object?)request.TabelaPrecoID ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@FlagPrecoConformeTabela", request.FlagPrecoConformeTabela ? 1 : 0);
        insertCmd.Parameters.AddWithValue("@UfOrigem",                (object?)request.UfOrigem ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@UfDestino",               (object?)request.UfDestino ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@CodigoIBGE",              (object?)request.CodigoIBGE ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@MargemPadrao",            (object?)request.MargemPadrao ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@DataValidade",            (object?)request.DataValidade ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@CondPagto",               (object?)request.CondPagtoId ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@FormaPagamentoSAP",       (object?)request.FormaPagamentoSAP ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TipoOVSAP",               (object?)request.TipoOVSAP ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@OrdemCompra",             (object?)request.OrdemCompra ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@NrContrato",              (object?)request.NrContrato ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TipoMotivoIDSAP",         (object?)request.TipoMotivoIDSAP ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ContatoNome",             (object?)request.ContatoNome ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ContatoEmail",            (object?)request.ContatoEmail ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@Obs",                     (object?)request.Obs ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@UsuarioId",               request.UsuarioId);
        insertCmd.Parameters.AddWithValue("@ValorVendaTotal",         request.ValorVendaTotal);
        insertCmd.Parameters.AddWithValue("@Frete",                   request.Frete);
        insertCmd.Parameters.AddWithValue("@VlrPedidoMinimo",         request.VlrPedidoMinimo);

        var propostaId = (int)(await insertCmd.ExecuteScalarAsync(cancellationToken))!;

        var prefixo = request.TipoID == 2 ? "CT" : "PR";
        var sufixo = request.EstabelecimentoID switch
        {
            1 => "MTZ", 2 => "FSP", 3 => "TSL", 4 => "TPA",
            5 => "BPN", 6 => "FBR", 7 => "SPA", 8 => "KPX", 9 => "STP",
            _ => string.Empty
        };
        var cdProposta = $"{prefixo}{propostaId:D6}{sufixo}";

        await using var updateCmd = new SqlCommand(updateCdProposta, connection);
        updateCmd.Parameters.AddWithValue("@CdProposta", cdProposta);
        updateCmd.Parameters.AddWithValue("@PropostaId", propostaId);
        await updateCmd.ExecuteNonQueryAsync(cancellationToken);

        return propostaId;
    }

    public async Task AtualizarPropostaAsync(
        int propostaId, CriarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BrWeb.dbo.Proposta SET
                Nome                    = @Nome,
                TipoCotacao             = @TipoCotacao,
                EstabelecimentoID       = @EstabelecimentoID,
                ClienteId               = @ClienteId,
                ClienteEnderecoID       = @ClienteEnderecoID,
                ClienteLocalEntregaID   = @ClienteLocalEntregaID,
                ObsLocalEntrega         = @ObsLocalEntrega,
                TabelaPrecoID           = @TabelaPrecoID,
                FlagPrecoConformeTabela = @FlagPrecoConformeTabela,
                UfOrigem                = @UfOrigem,
                UfDestino               = @UfDestino,
                CodigoIBGE              = @CodigoIBGE,
                MargemPadrao            = @MargemPadrao,
                DataValidade            = @DataValidade,
                CondPagto               = @CondPagto,
                FormaPagamentoSAP       = @FormaPagamentoSAP,
                tipoOVSAP               = @TipoOVSAP,
                OrdemCompra             = @OrdemCompra,
                NrContrato              = @NrContrato,
                TipoMotivoIDSAP         = @TipoMotivoIDSAP,
                ContatoNome             = @ContatoNome,
                ContatoEmail            = @ContatoEmail,
                Obs                     = @Obs
            WHERE PropostaId = @PropostaId
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaId",              propostaId);
        cmd.Parameters.AddWithValue("@Nome",                    (object?)request.Nome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TipoCotacao",             (object?)request.TipoNome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EstabelecimentoID",       request.EstabelecimentoID);
        cmd.Parameters.AddWithValue("@ClienteId",               request.ClienteId);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID",       (object?)request.ClienteEnderecoID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ClienteLocalEntregaID",   (object?)request.ClienteLocalEntregaID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ObsLocalEntrega",         (object?)request.ObsLocalEntrega ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TabelaPrecoID",           (object?)request.TabelaPrecoID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FlagPrecoConformeTabela", request.FlagPrecoConformeTabela ? 1 : 0);
        cmd.Parameters.AddWithValue("@UfOrigem",                (object?)request.UfOrigem ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UfDestino",               (object?)request.UfDestino ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CodigoIBGE",              (object?)request.CodigoIBGE ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MargemPadrao",            (object?)request.MargemPadrao ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataValidade",            (object?)request.DataValidade ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CondPagto",               (object?)request.CondPagtoId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FormaPagamentoSAP",       (object?)request.FormaPagamentoSAP ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TipoOVSAP",               (object?)request.TipoOVSAP ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OrdemCompra",             (object?)request.OrdemCompra ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NrContrato",              (object?)request.NrContrato ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TipoMotivoIDSAP",         (object?)request.TipoMotivoIDSAP ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContatoNome",             (object?)request.ContatoNome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContatoEmail",            (object?)request.ContatoEmail ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Obs",                     (object?)request.Obs ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CotacaoLocalEntregaOption>> EnsureLocaisEntregaAsync(
        int clienteEnderecoId, CancellationToken cancellationToken = default)
    {
        const string sqlSelect = """
            SELECT
                CLE.ClienteLocalEntregaID, CLE.FlagEnderecoDiferente, CLE.NmLocalEntrega,
                CLE.DsLogradouro, CLE.CdControle,
                UFLoc.CdUF, CidadeLoc.NmCidade AS Cidade,
                CE.Logradouro, UFEnd.CdUF AS CdUFEndereco, CE.Cidade AS CidadeEndereco,
                CLE.ObsLocalEntrega, CE.Bairro, CE.CondPagtoID, CE.Numero, CE.tipoOVSAP
            FROM BrSupply.dbo.BR_ClienteLocalEntrega CLE WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Cidade CidadeLoc WITH (NOLOCK) ON CidadeLoc.CidadeID = CLE.CdCidadeID
            LEFT JOIN BrSupply.dbo.BR_UF UFLoc WITH (NOLOCK) ON UFLoc.UFID = CLE.CdUFID
            LEFT JOIN BrSupply.dbo.BR_ClienteEndereco CE WITH (NOLOCK) ON CE.ClienteEnderecoID = CLE.ClienteEnderecoID
            LEFT JOIN BrSupply.dbo.BR_UF UFEnd WITH (NOLOCK) ON UFEnd.UFID = CE.UFID
            WHERE CLE.ClienteEnderecoID = @ClienteEnderecoID AND CLE.FlagAtivo = 1
            ORDER BY CLE.NmLocalEntrega, CE.Logradouro
            """;

        const string sqlInsert = """
            INSERT INTO BrSupply.dbo.BR_ClienteLocalEntrega (
                CdControle, NmLocalEntrega, CanalVendaID, ClienteEnderecoID,
                FlagAtivo, FlagHabilitado, FlagAceitaRegra, FlagEnderecoDiferente,
                VlrVerbaMensal, VlrUtilizado, VlrPendente
            )
            VALUES ('1', 'Recebimento / Almox', 3, @ClienteEnderecoID, 1, 1, 1, 0, 0, 0, 0)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var items = await ReadLocaisEntregaForEnsureAsync(connection, sqlSelect, clienteEnderecoId, cancellationToken);

        if (items.Count == 0)
        {
            await using var insertCmd = new SqlCommand(sqlInsert, connection);
            insertCmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);
            await insertCmd.ExecuteNonQueryAsync(cancellationToken);
            items = await ReadLocaisEntregaForEnsureAsync(connection, sqlSelect, clienteEnderecoId, cancellationToken);
        }

        return items;
    }

    private static async Task<List<CotacaoLocalEntregaOption>> ReadLocaisEntregaForEnsureAsync(
        SqlConnection connection, string sql, int clienteEnderecoId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);

        var items = new List<CotacaoLocalEntregaOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var nmLocal    = ReadString(reader, "NmLocalEntrega");
            var logradouro = reader.IsDBNull(reader.GetOrdinal("Logradouro")) ? ReadString(reader, "DsLogradouro") : ReadString(reader, "Logradouro");
            var cdUF       = reader.IsDBNull(reader.GetOrdinal("CdUF"))
                ? (reader.IsDBNull(reader.GetOrdinal("CdUFEndereco")) ? null : ReadString(reader, "CdUFEndereco"))
                : ReadString(reader, "CdUF");
            var cidade     = reader.IsDBNull(reader.GetOrdinal("Cidade"))
                ? (reader.IsDBNull(reader.GetOrdinal("CidadeEndereco")) ? null : ReadString(reader, "CidadeEndereco"))
                : ReadString(reader, "Cidade");
            var cdControle = ReadString(reader, "CdControle");
            items.Add(new CotacaoLocalEntregaOption
            {
                ClienteLocalEntregaId = reader.GetInt32(reader.GetOrdinal("ClienteLocalEntregaID")),
                Text                  = $"{nmLocal} - {logradouro} ({cdControle})",
                Logradouro            = logradouro,
                CdUF                  = cdUF,
                Cidade                = cidade,
                FlagEnderecoDiferente = reader.GetInt32(reader.GetOrdinal("FlagEnderecoDiferente")),
                CdControle            = cdControle,
                ObsLocalEntrega       = reader.IsDBNull(reader.GetOrdinal("ObsLocalEntrega")) ? null : ReadString(reader, "ObsLocalEntrega"),
                TipoOVSAP             = reader.IsDBNull(reader.GetOrdinal("tipoOVSAP")) ? null : ReadString(reader, "tipoOVSAP"),
                CondPagtoId           = reader.IsDBNull(reader.GetOrdinal("CondPagtoID")) ? null : reader.GetInt32(reader.GetOrdinal("CondPagtoID"))
            });
        }
        return items;
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    public async Task SalvarLogEnvioAsync(
        SalvarLogEnvioRequest p, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO BRWeb..Proposta_CotacaoEnvio
                (PropostaID, Nome, Email, Saudacao, Mensagem, ComCopia, Hash,
                 UsuarioID,
                 FlagVisualizaEstoque, FlagPodeTrocarTransportadora,
                 FlagPodeTrocarCondPagto, FlagPodeNegociar,
                 DataHora, FlagAtivo)
            VALUES
                (@PropostaID, @Nome, @Email, @Saudacao, @Mensagem, @ComCopia, @Hash,
                 @UsuarioID,
                 @FlagVisualizaEstoque, @FlagPodeTrocarTransportadora,
                 @FlagPodeTrocarCondPagto, @FlagPodeNegociar,
                 GETDATE(), 1)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaID",                  p.PropostaId);
        cmd.Parameters.AddWithValue("@Nome",                        p.Nome);
        cmd.Parameters.AddWithValue("@Email",                       p.Email);
        cmd.Parameters.AddWithValue("@Saudacao",                    p.Saudacao);
        cmd.Parameters.AddWithValue("@Mensagem",                    (object?)p.Mensagem ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ComCopia",                    (object?)p.ComCopia ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Hash",                        p.Hash);
        cmd.Parameters.AddWithValue("@UsuarioID",                   p.UsuarioId);
        cmd.Parameters.AddWithValue("@FlagVisualizaEstoque",        p.PodeDispEstoque);
        cmd.Parameters.AddWithValue("@FlagPodeTrocarTransportadora", p.PodeAltTransportadora);
        cmd.Parameters.AddWithValue("@FlagPodeTrocarCondPagto",     p.PodeAltCondPagamento);
        cmd.Parameters.AddWithValue("@FlagPodeNegociar",            p.PodeNegociar);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}