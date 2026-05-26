using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities.Liberacao;
using System.Data;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlLiberacaoPedidoQueryRepository(IConfiguration configuration) : ILiberacaoPedidoQueryRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<LiberacaoPedidoComboItem>> ListarCanaisVendaAsync(int usuarioId, string nmCanalVendaAtual, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CV.CanalVendaID, CV.NmCanalVenda
            FROM BR_CanalVenda CV (NOLOCK)
            JOIN BR_PedidoCanalVenda PCV (NOLOCK) ON PCV.CanalVendaID = CV.CanalVendaID
            WHERE PCV.FlagAtivo = 1
              AND CV.NmCanalVenda <> @NmCanalAtual
              AND PCV.UsuarioID = @UsuarioID
            ORDER BY CV.NmCanalVenda;";

        return await ReadComboAsync(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@NmCanalAtual", nmCanalVendaAtual ?? string.Empty);
        }, "CanalVendaID", "NmCanalVenda", cancellationToken);
    }

    public async Task<IReadOnlyList<LiberacaoPedidoComboItem>> ListarCategoriasAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT C.ClienteCategoriaPedidoID, C.NmCategoria
            FROM BR_ClienteCategoriaPedido C (NOLOCK)
            WHERE C.ClienteID = @ClienteID
              AND C.FlagAtivo = 1
            ORDER BY C.NmCategoria;";

        return await ReadComboAsync(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@ClienteID", clienteId);
        }, "ClienteCategoriaPedidoID", "NmCategoria", cancellationToken);
    }

    public async Task<IReadOnlyList<LiberacaoPedidoComboItem>> ListarCondicoesPagamentoAsync(string nmCondPagtoAtual, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT C.CondPagtoID, C.NmCondPagto
            FROM BR_CondPagto C (NOLOCK)
            WHERE C.NmCondPagto <> @NmCondPagtoAtual
              AND C.FlagPagarReceber = 'R'
              AND C.FlagAtivo = 1
              AND LEN(C.NmCondPagto) > 1
            ORDER BY C.NmCondPagto;";

        return await ReadComboAsync(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@NmCondPagtoAtual", nmCondPagtoAtual ?? string.Empty);
        }, "CondPagtoID", "NmCondPagto", cancellationToken);
    }

    public async Task<IReadOnlyList<LiberacaoPedidoFreteOpcao>> ListarOpcoesFreteAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT NomeTransportadora,
                   ValorFrete,
                   PrazoLogistico,
                   PrazoComercial,
                   TaxaExtra,
                   QtItensRestritos,
                   FlagClienteFixo,
                   FlagObrigatoriaCanalVenda,
                   FlagClienteRestrito
            FROM BR_LogisticaCalculoFrete (NOLOCK)
            WHERE CotacaoID = @CotacaoID
            ORDER BY ValorFrete ASC;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        var items = new List<LiberacaoPedidoFreteOpcao>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LiberacaoPedidoFreteOpcao
            {
                NomeTransportadora = GetString(reader, "NomeTransportadora"),
                ValorFrete = GetDecimal(reader, "ValorFrete"),
                PrazoLogistico = GetInt(reader, "PrazoLogistico"),
                PrazoComercial = GetInt(reader, "PrazoComercial"),
                TaxaExtra = GetDecimal(reader, "TaxaExtra"),
                QtItensRestritos = GetInt(reader, "QtItensRestritos"),
                FlagClienteFixo = GetInt(reader, "FlagClienteFixo"),
                FlagObrigatoriaCanalVenda = GetInt(reader, "FlagObrigatoriaCanalVenda"),
                FlagClienteRestrito = GetInt(reader, "FlagClienteRestrito")
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<LiberacaoPedidoImpostoItem>> ListarImpostosAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CI.itemDocumentoSAP,
                   I.CdItem,
                   SUBSTRING(I.NmItem, 1, 10) AS NmItemAbrev,
                   CI.MKUP,
                   CONVERT(INT, CI.QtItem) AS QtItem,
                   CI.VlrUnitario,
                   CI.MargemEnviada,
                   CI.MargemCalculada,
                   CI.PercentualICMS,
                   CI.PercentualFCP,
                   CI.PercentualIPI,
                   CI.PercentualCOFINS,
                   CI.PercentualPIS,
                   CI.ValorICMS,
                   CI.ValorIPI,
                   CI.ValorST,
                   CI.ValorISS,
                   CI.ValorCOFINS,
                   CI.ValorPIS,
                   CI.ValorFundoCombPobreza,
                   CI.ValorICMSPartilhaOrigem,
                   CI.ValorICMSPartilhaDestino,
                   CI.LB,
                   CI.ROL
            FROM BrSupply.dbo.BR_CotacaoItem CI (NOLOCK)
            JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = CI.ItemID
            WHERE CI.CotacaoID = @CotacaoID
            ORDER BY CI.itemDocumentoSAP DESC;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        var items = new List<LiberacaoPedidoImpostoItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LiberacaoPedidoImpostoItem
            {
                ItemDocumentoSAP = GetString(reader, "itemDocumentoSAP"),
                CdItem = GetString(reader, "CdItem"),
                NmItemAbrev = GetString(reader, "NmItemAbrev"),
                QtItem = GetInt(reader, "QtItem"),
                VlrUnitario = GetDecimal(reader, "VlrUnitario"),
                MKUP = GetDecimal(reader, "MKUP"),
                MargemCalculada = GetDecimal(reader, "MargemCalculada"),
                PercentualICMS = GetDecimal(reader, "PercentualICMS"),
                ValorICMS = GetDecimal(reader, "ValorICMS"),
                PercentualIPI = GetDecimal(reader, "PercentualIPI"),
                ValorIPI = GetDecimal(reader, "ValorIPI"),
                PercentualPIS = GetDecimal(reader, "PercentualPIS"),
                ValorPIS = GetDecimal(reader, "ValorPIS"),
                PercentualCOFINS = GetDecimal(reader, "PercentualCOFINS"),
                ValorCOFINS = GetDecimal(reader, "ValorCOFINS"),
                PercentualFCP = GetDecimal(reader, "PercentualFCP"),
                ValorFundoCombPobreza = GetDecimal(reader, "ValorFundoCombPobreza"),
                ValorST = GetDecimal(reader, "ValorST"),
                ValorISS = GetDecimal(reader, "ValorISS"),
                ValorICMSPartilhaOrigem = GetDecimal(reader, "ValorICMSPartilhaOrigem"),
                ValorICMSPartilhaDestino = GetDecimal(reader, "ValorICMSPartilhaDestino"),
                LB = GetDecimal(reader, "LB"),
                ROL = GetDecimal(reader, "ROL")
            });
        }
        return items;
    }

    // ======================================================================
    //  ITENS (Fase 5)
    // ======================================================================

    public async Task<IReadOnlyList<LiberacaoPedidoItemBrSupply>> ListarItensBrSupplyAsync(int cotacaoId, CancellationToken ct = default)
    {
        // Conversão fiel do SELECT do PHP comercial_liberacao_pedido.php (bloco "Itens Br Supply").
        const string sql = @"
            DECLARE @CotacaoID INT = @pCotacaoID;

            DECLARE @TableRupturas TABLE(
                FlagRuptura INT,
                ItemID INT,
                QtDisponivel INT,
                VlrCusto NUMERIC(18,2),
                Previsao VARCHAR(100)
            );

            INSERT INTO @TableRupturas
            SELECT 1,
                   I.ItemID,
                   CONVERT(INTEGER, (ISNULL(P.QtDispEstoque,0) - ISNULL(P.QtAlocadaSemOV,0))) AS QtDisponivel,
                   ISNULL(P.VlrCustoAquisicao, P.VlrCustoMedio) AS VlrCusto,
                   (SELECT TOP 1 CONVERT(VARCHAR(10), E.DtPrevEntrega, 103) + ' | ' + CONVERT(Varchar, CONVERT(Int, QtItemCompra)) + ' Und'
                      FROM BrSupply.dbo.BR_ItemEntrega E (NOLOCK)
                     WHERE EstabelecimentoID = X.EstabelecimentoID
                       AND ItemID = I.ItemID
                     ORDER BY DtPrevEntrega) AS Previsao
              FROM BrSupply.dbo.BR_Cotacao X (NOLOCK)
              JOIN BrSupply.dbo.BR_CotacaoItem C (NOLOCK) ON C.CotacaoID = X.CotacaoID
              JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = C.ItemID
              JOIN BrSupply.dbo.BR_PrecoEstoque P (NOLOCK) ON P.ItemID = I.ItemID AND P.EstabelecimentoID = X.EstabelecimentoID
             WHERE X.StatusCotacao NOT IN (1,2,4,9,17,18,19)
               AND C.QtItem > (ISNULL(P.QtDispEstoque,0) - ISNULL(P.QtAlocadaSemOV,0))
               AND ISNULL(C.FlagAlocaPedido,0) = 0
               AND C.CotacaoID = @CotacaoID;

            SELECT C.MargemCalculada AS MargemCalculada,
                   C.CotacaoItemID,
                   I.ItemID,
                   I.CdItem,
                   I.NmItem,
                   ISNULL(IIF(SAP.OrdemVenda = '', 'Em Fila', SAP.OrdemVenda), '') AS OrdemVenda,
                   CONVERT(INT, C.QtItem) AS QtItem,
                   C.VlrFinal AS VlrUnit,
                   ISNULL(R.VlrCusto, 0) AS VlrCusto,
                   (C.QtItem * C.VlrFinal) AS VlrTotal,
                   CASE ISNULL(C.OrdemCliente, '')
                       WHEN '' THEN ''
                       ELSE C.OrdemCliente + ' / ' + ISNULL(C.SequenciaCliente, '')
                   END AS OrdemCliente,
                   ISNULL(C.OrdemCliente, '') AS Ordem,
                   ISNULL(C.SequenciaCliente, '') AS Sequencia,
                   CASE ISNULL(C.FlagAlocaPedido, 0)
                       WHEN 0 THEN CASE ISNULL(C.FlagAtendimentoManager, 0)
                                       WHEN 0 THEN 'Bloqueado'
                                       WHEN 1 THEN 'Não Alocado'
                                   END
                       WHEN 1 THEN 'Alocado'
                       WHEN 2 THEN 'Atendido'
                   END AS SituacaoItem,
                   CASE ISNULL(C.FlagAlocaPedido, 0)
                       WHEN 0 THEN CASE ISNULL(C.FlagAtendimentoManager, 0)
                                       WHEN 0 THEN 2
                                       WHEN 1 THEN 1
                                   END
                       WHEN 1 THEN 2
                       WHEN 2 THEN 4
                   END AS OrderBy,
                   ISNULL(C.FlagAlocaPedido, 0) AS FlagAlocaPedido,
                   CASE WHEN ((SELECT TOP 1 P.DtFollowCompras
                                 FROM BrSupply.dbo.BR_PrecoEstoque P (NOLOCK)
                                WHERE P.EstabelecimentoID = O.EstabelecimentoID
                                  AND P.ItemID = I.ItemID) >= GETDATE())
                        THEN 'Follow de Compras: ' + ISNULL((SELECT TOP 1 P.DsFollowCompras
                                                               FROM BrSupply.dbo.BR_PrecoEstoque P (NOLOCK)
                                                              WHERE P.EstabelecimentoID = O.EstabelecimentoID
                                                                AND P.ItemID = I.ItemID), '')
                        ELSE ''
                   END AS DsFollowCompras,
                   C.NaturezaOperacaoID,
                   FORMAT(C.MargemCalculada, 'N', 'pt-br') + '%' AS Margem,
                   ISNULL(R.Previsao, '') AS Previsao,
                   ISNULL(R.FlagRuptura, 0) AS FlagRuptura,
                   ISNULL(R.QtDisponivel, 0) AS QtDisponivel
              FROM BrSupply.dbo.BR_CotacaoItem C (NOLOCK)
              JOIN BrSupply.dbo.BR_Cotacao O (NOLOCK) ON O.CotacaoID = C.CotacaoID
              JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = C.ItemID
              LEFT JOIN Integracao_Clientes.dbo.BR_SAP_Pedidos SAP (NOLOCK) ON SAP.CotacaoID = C.CotacaoID AND SAP.NrPedCli LIKE 'P1%'
              LEFT JOIN @TableRupturas R ON R.ItemID = C.ItemID
             WHERE C.CotacaoID = @CotacaoID
             ORDER BY ISNULL(C.FlagAlocaPedido, 0), I.NmItem;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        cmd.Parameters.AddWithValue("@pCotacaoID", cotacaoId);

        var items = new List<LiberacaoPedidoItemBrSupply>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new LiberacaoPedidoItemBrSupply
            {
                MargemCalculada = GetDecimalNullable(reader, "MargemCalculada"),
                CotacaoItemID = GetInt(reader, "CotacaoItemID"),
                ItemID = GetInt(reader, "ItemID"),
                CdItem = GetString(reader, "CdItem"),
                NmItem = GetString(reader, "NmItem"),
                OrdemVenda = GetString(reader, "OrdemVenda"),
                QtItem = GetInt(reader, "QtItem"),
                VlrUnit = GetDecimal(reader, "VlrUnit"),
                VlrCusto = GetDecimal(reader, "VlrCusto"),
                VlrTotal = GetDecimal(reader, "VlrTotal"),
                OrdemCliente = GetString(reader, "OrdemCliente"),
                Ordem = GetString(reader, "Ordem"),
                Sequencia = GetString(reader, "Sequencia"),
                SituacaoItem = GetString(reader, "SituacaoItem"),
                OrderBy = GetInt(reader, "OrderBy"),
                FlagAlocaPedido = GetInt(reader, "FlagAlocaPedido"),
                DsFollowCompras = GetString(reader, "DsFollowCompras"),
                NaturezaOperacaoID = GetInt(reader, "NaturezaOperacaoID"),
                Margem = GetString(reader, "Margem"),
                Previsao = GetString(reader, "Previsao"),
                FlagRuptura = GetInt(reader, "FlagRuptura"),
                QtDisponivel = GetInt(reader, "QtDisponivel")
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<LiberacaoPedidoItemMarketplace>> ListarItensMarketplaceAsync(int cotacaoId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT C.CotacaoItemID,
                   I.CdItem,
                   I.NmItem,
                   CONVERT(INT, C.QtItem) AS QtItem,
                   C.VlrFinal AS VlrUnit,
                   (C.QtItem * C.VlrFinal) AS VlrTotal,
                   F.NmFornecedor
              FROM BrSupply.dbo.BR_CotacaoItem C (NOLOCK)
              JOIN BrSupply.dbo.BR_ItemFornecedor I (NOLOCK) ON I.ItemFornecedorID = C.ItemFornecedorID
              JOIN BrSupply.dbo.BR_Fornecedor F (NOLOCK) ON F.FornecedorID = I.FornecedorID
             WHERE C.CotacaoID = @CotacaoID
             ORDER BY F.NmFornecedor, I.NmItem;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        var items = new List<LiberacaoPedidoItemMarketplace>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new LiberacaoPedidoItemMarketplace
            {
                CotacaoItemID = GetInt(reader, "CotacaoItemID"),
                CdItem = GetString(reader, "CdItem"),
                NmItem = GetString(reader, "NmItem"),
                QtItem = GetInt(reader, "QtItem"),
                VlrUnit = GetDecimal(reader, "VlrUnit"),
                VlrTotal = GetDecimal(reader, "VlrTotal"),
                NmFornecedor = GetString(reader, "NmFornecedor")
            });
        }
        return items;
    }

    /// <summary>
    /// Busca itens compatíveis para troca — conversão fiel de comercial_ajax_buscar_itens_compativeis.php.
    /// Executa: (1) flag do cliente, (2) SP SIC_Itens_Compativeis_Troca, (3) SP SIC_Itens_Compativeis_Troca_Analise.
    /// </summary>
    public async Task<LiberacaoPedidoTrocaCompativeisResultado> BuscarCompativeisTrocaAsync(int cotacaoItemId, CancellationToken ct = default)
    {
        var resultado = new LiberacaoPedidoTrocaCompativeisResultado();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        // 1) Flag do cliente (permite exibir switch de troca automática)
        const string sqlFlag = @"
            SELECT ISNULL(C.FlagTrocaItemAutomatica, 0) AS FlagTrocaItemAutomatica
              FROM BrSupply.dbo.BR_ClienteConfig C (NOLOCK)
             WHERE C.ClienteID = (
                    SELECT A.ClienteID
                      FROM BrSupply.dbo.BR_Cotacao A (NOLOCK)
                     WHERE A.CotacaoID = (
                            SELECT B.CotacaoID
                              FROM BrSupply.dbo.BR_CotacaoItem B (NOLOCK)
                             WHERE B.CotacaoItemID = @CotacaoItemID));";

        await using (var cmd = new SqlCommand(sqlFlag, connection))
        {
            cmd.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
            var v = await cmd.ExecuteScalarAsync(ct);
            if (v is not null and not DBNull)
                resultado.FlagTrocaItemAutomatica = Convert.ToInt32(v);
        }

        // 2) Itens compatíveis (SP)
        var itens = new List<LiberacaoPedidoItemCompativel>();
        await using (var cmdSp = new SqlCommand("BrSupply.dbo.SIC_Itens_Compativeis_Troca", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure,
            CommandTimeout = 120
        })
        {
            cmdSp.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
            await using var reader = await cmdSp.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                itens.Add(new LiberacaoPedidoItemCompativel
                {
                    ItemID = GetInt(reader, "ItemID"),
                    CdItem = GetString(reader, "CdItem"),
                    NmItem = GetString(reader, "NmItem"),
                    VlrCusto = GetDecimal(reader, "VlrCusto"),
                    NCM = GetString(reader, "NCM"),
                    QtEstoqueDisponivel = GetInt(reader, "QtEstoqueDisponivel"),
                    ChaveTributacao = GetString(reader, "ChaveTributacao"),
                    VlrTabelaPreco = GetDecimal(reader, "VlrTabelaPreco")
                });
            }
        }
        resultado.Itens = itens;

        // 3) Mensagem de análise (SP)
        await using (var cmdAn = new SqlCommand("BrSupply.dbo.SIC_Itens_Compativeis_Troca_Analise", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure,
            CommandTimeout = 60
        })
        {
            cmdAn.Parameters.AddWithValue("@CotacaoItemID", cotacaoItemId);
            await using var reader = await cmdAn.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
                resultado.MensagemAnalise = GetString(reader, "MensagemAnalise");
        }

        return resultado;
    }

    // ======================================================================
    //  LOGS (Fase 4)
    // ======================================================================

    public async Task<IReadOnlyList<LiberacaoPedidoCotLog>> ListarCotLogAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT L.DtOperacao,
                   L.UsuarioID,
                   ISNULL(U.NmUsuario, '(desconhecido)') AS NmUsuario,
                   ISNULL(L.TipoOperacao, '') AS TipoOperacao,
                   ISNULL(L.Modificacao, '') AS Modificacao
              FROM BrSupply.dbo.BR_CotLog L (NOLOCK)
              LEFT JOIN BrSupply.dbo.BR_Usuario U (NOLOCK) ON U.UsuarioID = L.UsuarioID
             WHERE L.CotacaoID = @CotacaoID
             ORDER BY L.DtOperacao DESC;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        var items = new List<LiberacaoPedidoCotLog>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LiberacaoPedidoCotLog
            {
                DtOperacao = GetDateTime(reader, "DtOperacao"),
                UsuarioID = GetInt(reader, "UsuarioID"),
                NmUsuario = GetString(reader, "NmUsuario"),
                TipoOperacao = GetString(reader, "TipoOperacao"),
                Modificacao = GetString(reader, "Modificacao")
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<LiberacaoPedidoBackOfficeLog>> ListarBackOfficeLogAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT L.DataHora,
                   L.UsuarioID,
                   ISNULL(U.NmUsuario, '(desconhecido)') AS NmUsuario,
                   ISNULL(L.DsAcao, '') AS DsAcao,
                   ISNULL(L.Motivo, '') AS Motivo
              FROM Integracao_Clientes.dbo.BR_BackOfficeLog L (NOLOCK)
              LEFT JOIN BrSupply.dbo.BR_Usuario U (NOLOCK) ON U.UsuarioID = L.UsuarioID
             WHERE L.CotacaoID = @CotacaoID
             ORDER BY L.DataHora DESC;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        var items = new List<LiberacaoPedidoBackOfficeLog>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LiberacaoPedidoBackOfficeLog
            {
                DataHora = GetDateTime(reader, "DataHora"),
                UsuarioID = GetInt(reader, "UsuarioID"),
                NmUsuario = GetString(reader, "NmUsuario"),
                DsAcao = GetString(reader, "DsAcao"),
                Motivo = GetString(reader, "Motivo")
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<LiberacaoPedidoCotLogDetalhado>> ListarCotLogDetalhadoAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        // Join com BR_Item para exibir código/nome dos itens antigo e novo.
        const string sql = @"
            SELECT L.DataHora,
                   L.UsuarioID,
                   ISNULL(U.NmUsuario, '(desconhecido)') AS NmUsuario,
                   ISNULL(L.CotacaoItemID, 0) AS CotacaoItemID,
                   ISNULL(L.Operacao, '') AS Operacao,
                   L.OldItemID,
                   ISNULL(IO.CdItem, '') AS OldCdItem,
                   ISNULL(IO.NmItem, '') AS OldNmItem,
                   L.OldQtItem,
                   L.OldVlrFinal,
                   L.NewItemID,
                   ISNULL(IN2.CdItem, '') AS NewCdItem,
                   ISNULL(IN2.NmItem, '') AS NewNmItem,
                   L.NewQtItem,
                   L.NewVlrFinal,
                   ISNULL(L.Motivo, '') AS Motivo
              FROM Integracao_Clientes.dbo.BR_CotLogDetalhado L (NOLOCK)
              LEFT JOIN BrSupply.dbo.BR_Usuario U (NOLOCK) ON U.UsuarioID = L.UsuarioID
              LEFT JOIN BrSupply.dbo.BR_Item IO (NOLOCK) ON IO.ItemID = L.OldItemID
              LEFT JOIN BrSupply.dbo.BR_Item IN2 (NOLOCK) ON IN2.ItemID = L.NewItemID
             WHERE L.CotacaoID = @CotacaoID
             ORDER BY L.DataHora DESC;";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        var items = new List<LiberacaoPedidoCotLogDetalhado>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LiberacaoPedidoCotLogDetalhado
            {
                DataHora = GetDateTime(reader, "DataHora"),
                UsuarioID = GetInt(reader, "UsuarioID"),
                NmUsuario = GetString(reader, "NmUsuario"),
                CotacaoItemID = GetInt(reader, "CotacaoItemID"),
                Operacao = GetString(reader, "Operacao"),
                OldItemID = GetIntNullable(reader, "OldItemID"),
                OldCdItem = GetString(reader, "OldCdItem"),
                OldNmItem = GetString(reader, "OldNmItem"),
                OldQtItem = GetDecimalNullable(reader, "OldQtItem"),
                OldVlrFinal = GetDecimalNullable(reader, "OldVlrFinal"),
                NewItemID = GetIntNullable(reader, "NewItemID"),
                NewCdItem = GetString(reader, "NewCdItem"),
                NewNmItem = GetString(reader, "NewNmItem"),
                NewQtItem = GetDecimalNullable(reader, "NewQtItem"),
                NewVlrFinal = GetDecimalNullable(reader, "NewVlrFinal"),
                Motivo = GetString(reader, "Motivo")
            });
        }
        return items;
    }

    private async Task<IReadOnlyList<LiberacaoPedidoComboItem>> ReadComboAsync(
        string sql,
        Action<SqlCommand> applyParams,
        string idColumn,
        string nameColumn,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        applyParams(cmd);

        var items = new List<LiberacaoPedidoComboItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LiberacaoPedidoComboItem
            {
                Id = GetInt(reader, idColumn),
                Nome = GetString(reader, nameColumn)
            });
        }
        return items;
    }

    private static string GetString(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        return r.IsDBNull(idx) ? string.Empty : r.GetValue(idx)?.ToString()?.Trim() ?? string.Empty;
    }

    private static int GetInt(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        if (r.IsDBNull(idx)) return 0;
        return r.GetFieldType(idx) switch
        {
            var t when t == typeof(int) => r.GetInt32(idx),
            var t when t == typeof(short) => r.GetInt16(idx),
            var t when t == typeof(long) => (int)r.GetInt64(idx),
            var t when t == typeof(byte) => r.GetByte(idx),
            _ => Convert.ToInt32(r.GetValue(idx))
        };
    }

    private static decimal GetDecimal(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        if (r.IsDBNull(idx)) return 0m;
        return r.GetFieldType(idx) switch
        {
            var t when t == typeof(decimal) => r.GetDecimal(idx),
            var t when t == typeof(double) => (decimal)r.GetDouble(idx),
            var t when t == typeof(float) => (decimal)r.GetFloat(idx),
            _ => Convert.ToDecimal(r.GetValue(idx))
        };
    }

    private static decimal? GetDecimalNullable(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        return r.IsDBNull(idx) ? null : GetDecimal(r, col);
    }

    private static int? GetIntNullable(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        return r.IsDBNull(idx) ? null : GetInt(r, col);
    }

    private static DateTime GetDateTime(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        return r.IsDBNull(idx) ? default : r.GetDateTime(idx);
    }
}
