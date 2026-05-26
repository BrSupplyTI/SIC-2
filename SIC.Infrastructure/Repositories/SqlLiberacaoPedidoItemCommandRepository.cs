using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;

namespace SIC.Infrastructure.Repositories;

/// <summary>
/// Comandos de escrita de itens do pedido. Conversão fiel de comercial_liberacao_updates.php
/// (blocos ALTERAR_ITEM, ALTERAR_ITEM_COM_OV, EXCLUIR_ITEM, TROCAR_ITEM), parametrizados e transacionais.
/// Validações server-side (FlagAloca=2, OV existente, quantidade/valor positivos) preservadas fielmente.
/// </summary>
public sealed class SqlLiberacaoPedidoItemCommandRepository(IConfiguration configuration) : ILiberacaoPedidoItemCommandRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    // ======================================================================
    //  ALTERAR_ITEM
    // ======================================================================

    public async Task<string?> AlterarItemAsync(
        int cotacaoId, int cotacaoItemId, int itemIdOld, string cdItemOld, string nmItemOld,
        int qtNova, int qtAntiga, decimal vlrNovo, decimal vlrAntigo,
        string ordemNova, string ordemAntiga, string sequenciaNova, string sequenciaAntiga,
        string motivo, int usuarioId, CancellationToken ct = default)
    {
        // Validações espelhadas do PHP ------------------------------------
        if (qtNova <= 0)
            return $"A quantidade do item {cdItemOld} não pode ser alterada para 0 (zero) ou negativa.";
        if (vlrNovo <= 0)
            return $"O valor do item {cdItemOld} não pode ser alterado para 0 (zero) ou negativo.";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        // Validação 1: FlagAloca == 2 (atendido) → não pode alterar
        var flagAloca = await ObterFlagAlocaAsync(connection, cotacaoItemId, ct);
        if (flagAloca == 2)
            return $"O item {cdItemOld} já está atendido e não pode ser alterado.";

        // Validação 2: pedido já tem OV ou está na fila SAP → loga tentativa e nega
        var temOv = await PedidoTemOvOuFilaSapAsync(connection, cotacaoId, ct);
        if (temOv)
        {
            await LogarFalhaAlteracaoAsync(connection,
                cotacaoId, usuarioId, cdItemOld, nmItemOld,
                "Falha ao tentar alterar item do pedido",
                $"Falha ao tentar alterar o item {cdItemOld} - {nmItemOld}, de pedido que já possui OV ou está na fila de integração SAP",
                ct);
            return "Não é possível alterar este pedido. Ele já possui OV ou está na fila de integração SAP.";
        }

        // Monta descrição amigável das alterações
        var logItem = MontarLogAlteracao(
            cdItemOld, nmItemOld,
            qtNova, qtAntiga, vlrNovo, vlrAntigo,
            ordemNova, ordemAntiga, sequenciaNova, sequenciaAntiga);

        const string sql = @"
            UPDATE BrSupply.dbo.BR_CotacaoItem
               SET VlrFinal = @VlrNovo,
                   QtItem = @QtNova,
                   OrdemCliente = @OrdemNova,
                   SequenciaCliente = @SequenciaNova,
                   FlagAtendimentoManager = 1,
                   FlagAlocaPedido = 0
             WHERE CotacaoID = @CotacaoID
               AND CotacaoItemID = @CotacaoItemID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'A',
                    'Item Alterado pelo Consultor Comercial ' + @LogItem + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Alteração: ' + @LogItem + ' - Motivo: ' + @Motivo,
                    'Item Alterado pelo Consultor Comercial',
                    GETDATE(), @UsuarioID);

            INSERT INTO Integracao_Clientes.dbo.BR_CotLogDetalhado
                  (CotacaoID, CotacaoItemID, Operacao, OldItemID, OldQtItem, OldVlrFinal,
                   NewItemID, NewQtItem, NewVlrFinal, Motivo, UsuarioID, DataHora)
            VALUES (@CotacaoID, @CotacaoItemID, 'A', @ItemIDOld, @QtAntiga, @VlrAntigo,
                    @ItemIDOld, @QtNova, @VlrNovo, @Motivo, @UsuarioID, GETDATE());

            UPDATE BrSupply.dbo.BR_Cotacao SET
                   VlrFreteCalc = NULL,
                   PrazoEntregaCalc = NULL,
                   ObsCalcFrete = NULL,
                   DtProgEmbarque = NULL,
                   DtProgEntrega = NULL,
                   DtProgLiberacao = NULL,
                   PrazoEntregaTransp = NULL,
                   FlagNaoLiberaAutomatico = NULL
             WHERE CotacaoID = @CotacaoID
               AND StatusCotacao = 3;

            DECLARE @EstabID INT = (SELECT EstabelecimentoID FROM BrSupply.dbo.BR_Cotacao (NOLOCK) WHERE CotacaoID = @CotacaoID);
            DECLARE @ItID INT = (SELECT ItemID FROM BrSupply.dbo.BR_CotacaoItem (NOLOCK) WHERE CotacaoItemID = @CotacaoItemID);
            EXEC BrSupply.dbo.Correcao_Alocacoes_Estoque_Item @EstabID, @ItID;

            DECLARE @ClienteID INT = (SELECT ClienteID FROM BrSupply.dbo.BR_Cotacao WHERE CotacaoID = @CotacaoID);
            EXEC BrSupply.dbo.BRS_sp_CadBR_CalculaTaxaServico @ClienteID, @CotacaoID;

            EXEC BrSupply.dbo.CalcularMargensPedido @CotacaoID;";

        await ExecuteInTransactionAsync(connection, sql, ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@ItemIDOld", itemIdOld);
            cmd.Parameters.AddWithValue("@QtNova", qtNova);
            cmd.Parameters.AddWithValue("@QtAntiga", qtAntiga);
            cmd.Parameters.AddWithValue("@VlrNovo", vlrNovo);
            cmd.Parameters.AddWithValue("@VlrAntigo", vlrAntigo);
            cmd.Parameters.AddWithValue("@OrdemNova", (object?)ordemNova ?? string.Empty);
            cmd.Parameters.AddWithValue("@SequenciaNova", (object?)sequenciaNova ?? string.Empty);
            cmd.Parameters.AddWithValue("@LogItem", logItem);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
        });

        return null;
    }

    // ======================================================================
    //  ALTERAR_ITEM_COM_OV  (só ordem/sequência)
    // ======================================================================

    public async Task<string?> AlterarItemComOvAsync(
        int cotacaoId, int cotacaoItemId, string cdItemOld, string nmItemOld,
        string ordemNova, string ordemAntiga, string sequenciaNova, string sequenciaAntiga,
        string motivo, int usuarioId, CancellationToken ct = default)
    {
        if (ordemNova == ordemAntiga && sequenciaNova == sequenciaAntiga)
            return "A ordem e a sequência são as mesmas anteriores. Nenhum dado foi atualizado.";

        var logItem = $"| {cdItemOld} - {nmItemOld}";
        if (ordemNova != ordemAntiga)
            logItem += $" | Ordem Cliente de [{ordemAntiga}] para [{ordemNova}]";
        if (sequenciaNova != sequenciaAntiga)
            logItem += $" | Seq. Ordem Cliente de [{sequenciaAntiga}] para [{sequenciaNova}]";

        const string sql = @"
            UPDATE BrSupply.dbo.BR_CotacaoItem
               SET OrdemCliente = @OrdemNova,
                   SequenciaCliente = @SequenciaNova
             WHERE CotacaoID = @CotacaoID
               AND CotacaoItemID = @CotacaoItemID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'A',
                    'Item Alterado pelo Consultor Comercial ' + @LogItem + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Alteração: ' + @LogItem + ' | Motivo: ' + @Motivo,
                    'Item Alterado pelo Consultor Comercial',
                    GETDATE(), @UsuarioID);

            DECLARE @ClienteID INT = (SELECT ClienteID FROM BrSupply.dbo.BR_Cotacao WHERE CotacaoID = @CotacaoID);
            EXEC BrSupply.dbo.BRS_sp_CadBR_CalculaTaxaServico @ClienteID, @CotacaoID;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await ExecuteInTransactionAsync(connection, sql, ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@OrdemNova", (object?)ordemNova ?? string.Empty);
            cmd.Parameters.AddWithValue("@SequenciaNova", (object?)sequenciaNova ?? string.Empty);
            cmd.Parameters.AddWithValue("@LogItem", logItem);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
        });

        return null;
    }

    // ======================================================================
    //  EXCLUIR_ITEM
    // ======================================================================

    public async Task<string?> ExcluirItemAsync(
        int cotacaoId, int cotacaoItemId, int itemIdOld, string cdItemOld, string nmItemOld,
        decimal qtAntiga, decimal vlrAntigo,
        string motivo, int usuarioId, int estabelecimentoId,
        CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        var flagAloca = await ObterFlagAlocaAsync(connection, cotacaoItemId, ct);
        if (flagAloca == 2)
            return $"O item {cdItemOld} já está atendido e não pode ser excluído.";

        var temOv = await PedidoTemOvOuFilaSapAsync(connection, cotacaoId, ct);
        if (temOv)
        {
            await LogarFalhaAlteracaoAsync(connection,
                cotacaoId, usuarioId, cdItemOld, nmItemOld,
                "Falha ao tentar excluir item do pedido",
                $"Falha ao tentar excluir o item {cdItemOld} - {nmItemOld} de pedido que já possui OV ou está na fila de integração SAP",
                ct);
            return "Não é possível alterar este pedido. Ele já possui OV ou está na fila de integração SAP.";
        }

        var logItem = $" | Item {cdItemOld} - {nmItemOld} excluído do pedido | Quantidade: {qtAntiga:0} | Valor: R$ {vlrAntigo:N2}";

        const string sql = @"
            DELETE FROM BrSupply.dbo.BR_CotacaoItemNota
             WHERE CotacaoItemID = @CotacaoItemID;

            DELETE FROM BrSupply.dbo.BR_CotacaoItem
             WHERE CotacaoID = @CotacaoID
               AND CotacaoItemID = @CotacaoItemID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'E',
                    'Item Excluído pelo Consultor Comercial ' + @LogItem + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo da Exclusão: ' + @Motivo + ' ' + @LogItem,
                    'Item Excluído pelo Consultor Comercial',
                    GETDATE(), @UsuarioID);

            INSERT INTO Integracao_Clientes.dbo.BR_CotLogDetalhado
                  (CotacaoID, CotacaoItemID, Operacao, OldItemID, OldQtItem, OldVlrFinal, Motivo, UsuarioID, DataHora)
            VALUES (@CotacaoID, @CotacaoItemID, 'E', @ItemIDOld, @QtAntiga, @VlrAntigo, @Motivo, @UsuarioID, GETDATE());

            UPDATE BrSupply.dbo.BR_Cotacao SET
                   VlrFreteCalc = NULL,
                   PrazoEntregaCalc = NULL,
                   ObsCalcFrete = NULL,
                   DtProgEmbarque = NULL,
                   DtProgEntrega = NULL,
                   DtProgLiberacao = NULL,
                   PrazoEntregaTransp = NULL,
                   FlagNaoLiberaAutomatico = NULL
             WHERE CotacaoID = @CotacaoID
               AND StatusCotacao = 3;

            EXEC BrSupply.dbo.Correcao_Alocacoes_Estoque_Item @EstabID, @ItemIDOld;

            DECLARE @ClienteID INT = (SELECT ClienteID FROM BrSupply.dbo.BR_Cotacao WHERE CotacaoID = @CotacaoID);
            EXEC BrSupply.dbo.BRS_sp_CadBR_CalculaTaxaServico @ClienteID, @CotacaoID;

            EXEC BrSupply.dbo.CalcularMargensPedido @CotacaoID;";

        await ExecuteInTransactionAsync(connection, sql, ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@ItemIDOld", itemIdOld);
            cmd.Parameters.AddWithValue("@QtAntiga", qtAntiga);
            cmd.Parameters.AddWithValue("@VlrAntigo", vlrAntigo);
            cmd.Parameters.AddWithValue("@LogItem", logItem);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
            cmd.Parameters.AddWithValue("@EstabID", estabelecimentoId);
        });

        return null;
    }

    // ======================================================================
    //  TROCAR_ITEM
    // ======================================================================

    public async Task<string?> TrocarItemAsync(
        int cotacaoId, int cotacaoItemId, int itemIdOld, string cdItemOld, string nmItemOld,
        int itemSubstitutoId, bool flagTrocaAutomatica,
        string motivo, int usuarioId, int estabelecimentoId,
        CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        var flagAloca = await ObterFlagAlocaAsync(connection, cotacaoItemId, ct);
        if (flagAloca == 2)
            return $"O item {cdItemOld} já está atendido e não pode ser excluído.";

        // Identifica o novo item (para gerar a descrição do log)
        string cdItemNew = string.Empty, nmItemNew = string.Empty;
        await using (var cmdInfo = new SqlCommand(
            "SELECT CdItem, NmItem FROM BrSupply.dbo.BR_Item (NOLOCK) WHERE ItemID = @ItemID", connection))
        {
            cmdInfo.Parameters.AddWithValue("@ItemID", itemSubstitutoId);
            await using var r = await cmdInfo.ExecuteReaderAsync(ct);
            if (await r.ReadAsync(ct))
            {
                cdItemNew = r["CdItem"]?.ToString() ?? string.Empty;
                nmItemNew = r["NmItem"]?.ToString() ?? string.Empty;
            }
        }

        var logItem = $" O item {cdItemOld} - {nmItemOld} foi substituído pelo item {cdItemNew} - {nmItemNew}";
        if (flagTrocaAutomatica)
            logItem += " (Troca automática ativada)";

        // Bloco principal ------------------------------------------------
        var sqlPrincipal = @"
            UPDATE BrSupply.dbo.BR_CotacaoItem
               SET ItemID = @ItemSubstitutoID,
                   FlagAtendimentoManager = 1,
                   FlagAlocaPedido = 0
             WHERE CotacaoID = @CotacaoID
               AND CotacaoItemID = @CotacaoItemID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'A',
                    'Item Substituído pelo Consultor Comercial | ' + @LogItem + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    @LogItem + ' | Motivo: ' + @Motivo,
                    'Item Substituído pelo Consultor Comercial',
                    GETDATE(), @UsuarioID);

            INSERT INTO Integracao_Clientes.dbo.BR_CotLogDetalhado
                  (CotacaoID, CotacaoItemID, Operacao, OldItemID, NewItemID, Motivo, UsuarioID, DataHora)
            VALUES (@CotacaoID, @CotacaoItemID, 'T', @ItemIDOld, @ItemSubstitutoID, @Motivo, @UsuarioID, GETDATE());

            UPDATE BrSupply.dbo.BR_Cotacao SET
                   VlrFreteCalc = NULL,
                   PrazoEntregaCalc = NULL,
                   ObsCalcFrete = NULL,
                   DtProgEmbarque = NULL,
                   DtProgEntrega = NULL,
                   DtProgLiberacao = NULL,
                   PrazoEntregaTransp = NULL,
                   FlagNaoLiberaAutomatico = NULL
             WHERE CotacaoID = @CotacaoID
               AND StatusCotacao = 3;

            EXEC BrSupply.dbo.Correcao_Alocacoes_Estoque_Item @EstabID, @ItemSubstitutoID;
            EXEC BrSupply.dbo.Correcao_Alocacoes_Estoque_Item @EstabID, @ItemIDOld;

            DECLARE @ClienteID INT = (SELECT ClienteID FROM BrSupply.dbo.BR_Cotacao WHERE CotacaoID = @CotacaoID);
            EXEC BrSupply.dbo.BRS_sp_CadBR_CalculaTaxaServico @ClienteID, @CotacaoID;

            EXEC BrSupply.dbo.CalcularMargensPedido @CotacaoID;";

        // Se troca automática, grava a relação cliente↔item (se ainda não existe)
        if (flagTrocaAutomatica)
        {
            sqlPrincipal += @"

                IF NOT EXISTS (
                    SELECT 1
                      FROM BrSupply.dbo.BR_ItensTrocaAutomatica_PorCliente (NOLOCK)
                     WHERE ClienteID = @ClienteID
                       AND ItemIDRuptura = @ItemIDOld
                )
                BEGIN
                    INSERT INTO BrSupply.dbo.BR_ItensTrocaAutomatica_PorCliente
                        (Fator, DtHrRegistro, UsuarioID, ClienteID, ItemIDRuptura, ItemIDNovo)
                    VALUES (1, GETDATE(), @EstabID, @ClienteID, @ItemIDOld, @ItemSubstitutoID);
                END;";
        }

        await ExecuteInTransactionAsync(connection, sqlPrincipal, ct, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@ItemIDOld", itemIdOld);
            cmd.Parameters.AddWithValue("@ItemSubstitutoID", itemSubstitutoId);
            cmd.Parameters.AddWithValue("@LogItem", logItem);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
            cmd.Parameters.AddWithValue("@EstabID", estabelecimentoId);
        });

        return null;
    }

    // ======================================================================
    //  Helpers
    // ======================================================================

    private static async Task<int> ObterFlagAlocaAsync(SqlConnection connection, int cotacaoItemId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(
            @"SELECT ISNULL(FlagAlocaPedido, 0)
                FROM BrSupply.dbo.BR_CotacaoItem (NOLOCK)
               WHERE CotacaoItemID = @CotacaoItemID;", connection);
        cmd.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
        var v = await cmd.ExecuteScalarAsync(ct);
        return v is null or DBNull ? 0 : Convert.ToInt32(v);
    }

    private static async Task<bool> PedidoTemOvOuFilaSapAsync(SqlConnection connection, int cotacaoId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(
            @"SELECT COUNT(*)
                FROM Integracao_Clientes.dbo.BR_SAP_Pedidos (NOLOCK)
               WHERE CotacaoID = @CotacaoID
                 AND ((ISNULL(OrdemVenda,'') <> '')
                   OR (MsgRetorno = 'Novo Pedido'));", connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
        var v = await cmd.ExecuteScalarAsync(ct);
        return v is not null and not DBNull && Convert.ToInt32(v) > 0;
    }

    private static async Task LogarFalhaAlteracaoAsync(
        SqlConnection connection,
        int cotacaoId, int usuarioId, string _cdItemOld, string _nmItemOld,
        string dsAcao, string motivo,
        CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, DsAcao, Motivo, DataHora, UsuarioID)
            VALUES (@CotacaoID, @DsAcao, @Motivo, GETDATE(), @UsuarioID);";

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
        cmd.Parameters.AddWithValue("@DsAcao", dsAcao);
        cmd.Parameters.AddWithValue("@Motivo", motivo);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static string MontarLogAlteracao(
        string cdItemOld, string nmItemOld,
        int qtNova, int qtAntiga, decimal vlrNovo, decimal vlrAntigo,
        string ordemNova, string ordemAntiga, string sequenciaNova, string sequenciaAntiga)
    {
        var log = $"| {cdItemOld} - {nmItemOld}";
        if (qtNova != qtAntiga)
            log += $" | Quantidade alterada de {qtAntiga} para {qtNova} unidades.";
        if (vlrNovo != vlrAntigo)
            log += $" | Valor Unitário alterado de R$ {vlrAntigo:N2} para R$ {vlrNovo:N2}.";
        if (ordemNova != ordemAntiga)
            log += $" | Ordem Cliente de [{ordemAntiga}] para [{ordemNova}].";
        if (sequenciaNova != sequenciaAntiga)
            log += $" | Seq. Ordem Cliente de [{sequenciaAntiga}] para [{sequenciaNova}].";
        return log;
    }

    private static async Task ExecuteInTransactionAsync(
        SqlConnection connection, string sql, CancellationToken ct, Action<SqlCommand> applyParams)
    {
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(ct);
        try
        {
            await using var cmd = new SqlCommand(sql, connection, transaction) { CommandTimeout = 180 };
            applyParams(cmd);
            await cmd.ExecuteNonQueryAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
