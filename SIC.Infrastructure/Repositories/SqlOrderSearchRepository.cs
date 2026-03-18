using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;
using System.Data;
using System.Runtime.Intrinsics.Arm;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlOrderSearchRepository(IConfiguration configuration) : IOrderSearchRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<bool> ExistsOrderByNumberAsync(int numeroPedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM BR_Cotacao WITH (NOLOCK)
            WHERE CotacaoID = @numeroPedido;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@numeroPedido", numeroPedido);

        var count = (int)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);
        return count > 0;
    }

    public async Task<OrderHeaderDetails?> GetOrderHeaderDetailsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
                        
            DECLARE @StatusAuxiliar VARCHAR(100)
            DECLARE @DtPrevFollow DATE
            DECLARE @DtPrevisaoEntrega DATE                        
            DECLARE @QtItensBRSupply INT = 0
            DECLARE @QtItensTerceiros INT = 0
            DECLARE @QtItensRuptura INT = 0
            DECLARE @ValorItensBRSupply NUMERIC(18,2) = 0
            DECLARE @ValorItensTerceiros NUMERIC(18,2) = 0
            DECLARE @FlagIntegradoSAP INT = 0

            SELECT @QtItensBRSupply = SUM(QtItensBRSupply),
                   @QtItensTerceiros = SUM(QtItensTerceiros),
                   @QtItensRuptura = SUM(QtItensRuptura),
                   @ValorItensBRSupply = SUM(ValorItensBRSupply),
                   @ValorItensTerceiros = SUM(ValorItensTerceiros)
            FROM (
                SELECT 
                   CASE WHEN ISNULL(I.ItemID, 0) > 0 AND ISNULL(I.ItemFornecedorID, 0) = 0 THEN 1 ELSE 0 END AS QtItensBRSupply,
                   CASE WHEN ISNULL(I.ItemID, 0) = 0 AND ISNULL(I.ItemFornecedorID, 0) > 0 THEN 1 ELSE 0 END AS QtItensTerceiros,
                   CASE WHEN ISNULL(I.ItemID, 0) > 0 AND ISNULL(I.ItemFornecedorID, 0) = 0 AND ISNULL(I.FlagAlocaPedido,0) = 0 AND ISNULL(I.FlagAtendimentoManager,0) = 1 THEN 1 ELSE 0 END AS QtItensRuptura,
                   CASE WHEN ISNULL(I.ItemID, 0) > 0 AND ISNULL(I.ItemFornecedorID, 0) = 0 THEN (I.QtItem * I.VlrFinal) ELSE 0 END AS ValorItensBRSupply,
                   CASE WHEN ISNULL(I.ItemID, 0) = 0 AND ISNULL(I.ItemFornecedorID, 0) > 0 THEN (I.QtItem * I.VlrFinal) ELSE 0 END AS ValorItensTerceiros
                FROM BR_CotacaoItem I WITH (NOLOCK)      
                WHERE CotacaoID = @Pedido
            ) A;

            SELECT TOP 1
                @StatusAuxiliar = 
                    CASE WHEN CT.StatusCotacao = 5
                         THEN CASE WHEN CT.DtPlanejadaOperacao IS NULL THEN 'Aguardando Planejamento Operacional'
                                   WHEN CT.DtPlanejadaOperacao IS NOT NULL AND ISNULL((SELECT Y.CotacaoID 
                                                                                       FROM TmpLibPedido Y WITH (NOLOCK) 
                                                                                       WHERE Y.CotacaoID = CT.CotacaoID),0) = 0 THEN 'Aguardando Liberação no Painel Logístico' 
                                   ELSE 'Liberado no Painel Logístico' 
                              END 
                        ELSE CASE 
            	                WHEN ISNULL(RO.RomaneioHubID,0) > 0 AND CT.StatusCotacao IN (5, 29, 11, 16, 28) AND RO.TipoRomaneio = 1 AND RN.DtEntrega IS NULL THEN 'Em trânsito para o HUB ' + HU.NmHub
            		            WHEN ISNULL(RO.RomaneioHubID,0) > 0 AND CT.StatusCotacao IN (5, 29, 11, 16, 28) AND RO.TipoRomaneio = 1 AND RN.DtEntrega IS NOT NULL THEN 'Chegada no HUB ' + HU.NmHub		    
            		            WHEN ISNULL(RO.RomaneioHubID,0) > 0 AND CT.StatusCotacao IN (5, 29, 11, 16, 28) AND RO.TipoRomaneio != 1 AND RN.DtPortaria IS NOT NULL THEN 'Em Rota Para o Cliente'
            	                ELSE '' 
                             END
                    END,
                @DtPrevFollow = RN.DtPrevFollow,
                @DtPrevisaoEntrega = RN.DtPrevisaoEntrega
            FROM BR_Cotacao CT WITH (NOLOCK)
            JOIN BR_RomaneioNota RN WITH (NOLOCK) ON RN.CotacaoId = CT.CotacaoID
            JOIN BR_Romaneio RO WITH (NOLOCK) ON RO.RomaneioID = RN.RomaneioID	            
            LEFT JOIN BR_RomaneioHub HU WITH (NOLOCK) ON HU.RomaneioHubID = RO.RomaneioHubID	  
            WHERE CT.CotacaoID = @Pedido              
            ORDER BY RO.RomaneioID DESC;

            SELECT @FlagIntegradoSAP =
            	ISNULL((SELECT 1
            	         FROM Integracao_Clientes..BR_SAP_Pedidos WITH (NOLOCK)
            	         WHERE CotacaoID = @Pedido),0);

            SELECT C.CotacaoID as Pedido,
                   C.CompStatusCotacao,
                   C.DtCotacao AS DataPedido,
                   C.ClienteID,
                   I.LogoCliente,
                   ISNULL(I.LogoClienteDark, I.LogoCliente) AS LogoClienteDark,
                   W.NmEstabelecimento as Estabelecimento,
                   C.OrdemCompra,
                   A.NmCanalVenda as CanalVenda,
                   ISNULL(CA.NmCarteira,'') AS Carteira,
                   S.DsStatusCotacao as Situacao,
                   C.StatusCotacao as StatusID,
                   S.Setor,
                   ISNULL(CT.NmCategoria,'') AS NmCategoria,
                   ISNULL(CT.LabelInfoCategoria,'') AS LabelInfoCategoria,
                   ISNULL(C.InfoCategoria,'') AS InfoCategoria,
                   ISNULL(C.InfoCarrinho,'') AS InfoCarrinho,
                   ISNULL(I.LabelInfoCarrinho,'') AS LabelInfoCarrinho,
                   I.RazaoSocialCliente AS NmCliente,
                   I.CdExtCliente AS CodCliente,
                   I.CNPJCliente,
                   I.FlagTipoDocumento,
                   I.TelefoneCliente,
                   I.InscrEstCliente,
                   E.RazaoSocial AS RazaoSocialEndereco,
                   E.CdEms AS CodClienteEndereco,
                   E.CPFCNPJ,
                   E.Logradouro as RuaEndereco,
                   E.Numero as NumeroEndereco,
                   E.Complemento as ComplementoEndereco,
                   E.Bairro as BairroEndereco, 
                   (SMP.CodTipo + ' - ' + SMP.Descricao) AS MotivoOVSAP,
                   TDP.Descricao AS DescTipoOVSAP,
                   C.TipoOVSAP,
                   C.CotacaoIdOriginal,
                   ISNULL((SELECT TOP 1 CO.CotacaoID
                           FROM BR_Cotacao CO WITH (NOLOCK)
                           WHERE CO.CotacaoIDOriginal = C.CotacaoID),0) AS CotacaoIDSubstituta,
                   C.NrContrato,
                   C.MargemBruta,
            	   C.LB,
            	   C.ROL,
                   E.ClienteEnderecoID,
                   E.FlagTipoDocumento AS FlagTipoDocumentoEndereco,                   
                   CIDE.NmCidade AS CidadeEndereco,
                   UFE.CdUF AS UFEndereco,
                   CONVERT(VARCHAR(10), CIDE.CodigoIBGE) AS CidadeIBGEEndereco,
                   E.CEP AS CepEndereco,
                   ISNULL(L.FlagEnderecoDiferente,0) AS FlagEnderecoDirerente,
                   L.NmLocalEntrega as NmLocalEntrega,
                   L.CdControle,
                   L.ClienteLocalEntregaID,
                   L.DsLogradouro AS RuaLocal,
                   L.DsNumero AS NumeroLocal,
                   L.DsComplemento AS ComplementoLocal,
                   L.DsBairro AS BairroLocal,
                   CIDL.NmCidade AS CidadeLocal,
                   UFL.CdUF AS UFLocal,
                   CONVERT(VARCHAR(10), CIDL.CodigoIBGE) AS CidadeIBGELocal,
                   L.DsCEP AS CEPLocal,
                   ISNULL(SFP.Descricao,'') AS FormaPagto,
                   ISNULL(C.FlagFormaPagto,0) AS FlagFormaPagto,
                   ISNULL(P.NmCondPagto, '') AS CondPagto,
                   ISNULL(C.TidCielo, '') AS HashPagamento,
                   @StatusAuxiliar AS StatusAuxiliar,
                   ISNULL(UM.NmUsuario, USIC.NmUsuario) AS NmSolicitante,
                   ISNULL(UM.Email, USIC.Email) AS EmailSolicitante,
                   ISNULL(C.TransportadoraID,0) AS TransportadoraID,
                   T.NmTransportadora,
                   T.NrCNPJ AS CNPJTransportadora,             
                   ISNULL(C.VlrFreteCalc,0) AS VlrFreteCalc,
                   ISNULL(C.PrazoEntregaCalc,0) AS PrazoEntregaCalc,
                   ISNULL(C.PrazoEntregaTransp,0) AS PrazoEntregaTransp,
                   C.DtProgLiberacao,
                   C.DtProgEmbarque,
                   C.DtProgEntrega,
            	   C.DtPlanejadaOperacao,
                   C.DtSLACliente,
                   C.DtProgEmbFollow,
                   CASE ISNULL(C.AgrupadorFrete, 0) WHEN 0 THEN 'NÃO' ELSE 'SIM' END AS FreteAgrupado,        
                   ISNULL(C.ObsCalcFrete,'') AS ObsCalcFrete,
                   @DtPrevFollow AS DtPrevEntFollow,
                   @DtPrevisaoEntrega AS DtPrevisaoEntrega,
                   IIF(ISNULL(@DtPrevisaoEntrega,GETDATE()) > ISNULL(C.DtSLACliente, DATEADD(DAY,10, GETDATE())),'ATRASO', 'OK') AS StatusSLA,
                   C.ObsCotacao,
                   C.ObsAprovacao,
                   C.ObsNota,
                   L.ObsLocalEntrega,
                   ISNULL(C.VlrFrete,0) AS VlrFrete,
                   ISNULL(C.VlrTaxaServico, 0) AS VlrTaxaServico,
                   @QtItensBRSupply AS QtItensBRSupply,
                   @QtItensTerceiros AS QtItensTerceiros,
                   @QtItensRuptura AS QtItensRuptura,
                   @ValorItensBRSupply AS ValorItensBRSupply,
                   @ValorItensTerceiros AS ValorItensTerceiros,
                   @FlagIntegradoSAP AS FlagIntegradoSAP
            FROM BR_Cotacao C WITH (NOLOCK)
            JOIN BR_Estabelecimento W WITH (NOLOCK) ON W.EstabelecimentoID = C.EstabelecimentoID
            JOIN BR_Cliente I WITH (NOLOCK) ON I.ClienteID = C.ClienteID
            JOIN BR_CanalVenda A WITH (NOLOCK) ON A.CanalVendaID = C.CanalVendaID
            JOIN BR_StatusCotacao S WITH (NOLOCK) ON S.StatusCotacao = C.StatusCotacao
            JOIN BR_ClienteEndereco E WITH (NOLOCK) ON E.ClienteEnderecoID = C.ClienteEnderecoID
            JOIN BR_ClienteLocalEntrega L WITH (NOLOCK) ON L.ClienteLocalEntregaID = C.ClienteLocalEntregaID
            JOIN BR_Cidade CIDE WITH (NOLOCK) ON CIDE.CidadeID = E.CdCidadeEnderecoID
            JOIN BR_UF UFE WITH (NOLOCK) ON UFE.UFID = CIDE.UFID
            LEFT JOIN BR_CondPagto P WITH (NOLOCK) ON P.CondPagtoID = C.CondPagtoID
            LEFT JOIN BR_Transportadora T WITH (NOLOCK) ON T.TransportadoraID = C.TransportadoraID
            LEFT JOIN BR_Cidade CIDL WITH (NOLOCK) ON CIDL.CidadeID = L.CdCidadeID
            LEFT JOIN BR_UF UFL WITH (NOLOCK) ON UFL.UFID = CIDL.UFID
            LEFT JOIN BR_Carteira CA WITH (NOLOCK) ON CA.CarteiraID = I.CarteiraID
            LEFT JOIN BR_ClienteCategoriaPedido CT WITH (NOLOCK) ON CT.ClienteCategoriaPedidoID = C.ClienteCategoriaPedidoID
            LEFT JOIN Integracao_Clientes.dbo.BR_SAP_MotivosPedidos SMP WITH (NOLOCK) ON SMP.Id = C.TipoMotivoIDSAP
            LEFT JOIN Integracao_Clientes..BR_SAP_TiposDocumentosPedidos TDP WITH (NOLOCK) ON TDP.Tipo = C.tipoOVSAP                                       
            LEFT JOIN Integracao_Clientes.dbo.BR_SAP_FormasPagamento SFP ON SFP.ID = C.FormaPagamentoSAP         
            LEFT JOIN BR_CLienteUsuario UM WITH (NOLOCK) ON UM.ClienteUsuarioID = C.ClienteUsuarioID
            LEFT JOIN (SELECT TOP 1 
                            CAP.CotacaoID,
                            US.UsuarioID,
                            US.NmUsuario,
                            US.Email
                       FROM BR_CotAprov CAP WITH (NOLOCK) 
                       JOIN BR_Usuario US WITH (NOLOCK) ON US.UsuarioID = CAP.UsuarioID
                       WHERE CAP.CotacaoID = @Pedido 
                       ORDER BY CAP.CotAprovID) USIC ON USIC.CotacaoID = C.CotacaoID
            WHERE C.CotacaoID = @Pedido;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new OrderHeaderDetails
        {
            Pedido = reader.GetInt32(reader.GetOrdinal("Pedido")),
            CompStatusCotacao = ReadNullableString(reader, "CompStatusCotacao"),
            StatusAuxiliar = ReadNullableString(reader, "StatusAuxiliar"),
            DataPedido = ReadNullableDateTime(reader, "DataPedido"),
            Estabelecimento = ReadNullableString(reader, "Estabelecimento") ?? string.Empty,
            OrdemCompra = ReadNullableString(reader, "OrdemCompra") ?? string.Empty,
            CanalVenda = ReadNullableString(reader, "CanalVenda") ?? string.Empty,
            Carteira = ReadNullableString(reader, "Carteira") ?? string.Empty,
            Situacao = ReadNullableString(reader, "Situacao") ?? string.Empty,
            Setor = ReadNullableString(reader, "Setor") ?? string.Empty,
            StatusID = ReadNullableInt32(reader, "StatusID") ?? 0,
            Categoria = ReadNullableString(reader, "NmCategoria") ?? string.Empty,
            LabelInfoCategoria = ReadNullableString(reader, "LabelInfoCategoria") ?? string.Empty,
            InfoCategoria = ReadNullableString(reader, "InfoCategoria") ?? string.Empty,
            InfoCarrinho = ReadNullableString(reader, "InfoCarrinho") ?? string.Empty,
            LabelInfoCarrinho = ReadNullableString(reader, "LabelInfoCarrinho") ?? string.Empty,
            ClienteID = ReadNullableInt32(reader, "ClienteID") ?? 0,
            NomeCliente = ReadNullableString(reader, "NmCliente") ?? string.Empty,
            CodigoCliente = ReadNullableString(reader, "CodCliente") ?? string.Empty,
            CNPJCliente = ReadNullableString(reader, "CNPJCliente") ?? string.Empty,
            RazaoSocialEndereco = ReadNullableString(reader, "RazaoSocialEndereco") ?? string.Empty,
            CpfCnpj = ReadNullableString(reader, "CPFCNPJ") ?? string.Empty,
            CodClienteEndereco = ReadNullableString(reader, "CodClienteEndereco") ?? string.Empty,
            RuaEndereco = ReadNullableString(reader, "RuaEndereco") ?? string.Empty,
            NumeroEndereco = ReadNullableString(reader, "NumeroEndereco") ?? string.Empty,
            ComplementoEndereco = ReadNullableString(reader, "ComplementoEndereco") ?? string.Empty,
            BairroEndereco = ReadNullableString(reader, "BairroEndereco") ?? string.Empty,
            LogoCliente = ReadNullableString(reader, "LogoCliente") ?? string.Empty,
            LogoClienteDark = ReadNullableString(reader, "LogoClienteDark") ?? string.Empty,
            FlagTipoDocumento = ReadNullableString(reader, "FlagTipoDocumento") ?? string.Empty,
            TelefoneCliente = ReadNullableString(reader, "TelefoneCliente") ?? string.Empty,
            InscrEstCliente = ReadNullableString(reader, "InscrEstCliente") ?? string.Empty,
            MotivoOVSAP = ReadNullableString(reader, "MotivoOVSAP") ?? string.Empty,
            DescTipoOVSAP = ReadNullableString(reader, "DescTipoOVSAP") ?? string.Empty,
            TipoOVSAP = ReadNullableString(reader, "TipoOVSAP") ?? string.Empty,
            CotacaoIdOriginal = ReadNullableInt32(reader, "CotacaoIdOriginal") ?? 0,
            CotacaoIDSubstituta = ReadNullableInt32(reader, "CotacaoIDSubstituta") ?? 0,
            NrContrato = ReadNullableString(reader, "NrContrato") ?? string.Empty,
            MargemBruta = ReadNullableDecimal(reader, "MargemBruta") ?? 0,
            LB = ReadNullableDecimal(reader, "LB") ?? 0,
            ROL = ReadNullableDecimal(reader, "ROL") ?? 0,
            ClienteEnderecoID = ReadNullableInt32(reader, "ClienteEnderecoID") ?? 0,
            FlagTipoDocumentoEndereco = ReadNullableString(reader, "FlagTipoDocumentoEndereco") ?? string.Empty,
            CidadeEndereco = ReadNullableString(reader, "CidadeEndereco") ?? string.Empty,
            UFEndereco = ReadNullableString(reader, "UFEndereco") ?? string.Empty,
            CidadeIBGEEndereco = ReadNullableString(reader, "CidadeIBGEEndereco") ?? string.Empty,
            CepEndereco = ReadNullableString(reader, "CepEndereco") ?? string.Empty,
            FlagEnderecoDirerente = ReadNullableInt32(reader, "FlagEnderecoDirerente") ?? 0,
            NmLocalEntrega = ReadNullableString(reader, "NmLocalEntrega") ?? string.Empty,
            CdControle = ReadNullableString(reader, "CdControle") ?? string.Empty,
            ClienteLocalEntregaID = ReadNullableInt32(reader, "ClienteLocalEntregaID") ?? 0,
            RuaLocal = ReadNullableString(reader, "RuaLocal") ?? string.Empty,
            NumeroLocal = ReadNullableString(reader, "NumeroLocal") ?? string.Empty,
            ComplementoLocal = ReadNullableString(reader, "ComplementoLocal") ?? string.Empty,
            BairroLocal = ReadNullableString(reader, "BairroLocal") ?? string.Empty,
            CidadeLocal = ReadNullableString(reader, "CidadeLocal") ?? string.Empty,
            UFLocal = ReadNullableString(reader, "UFLocal") ?? string.Empty,
            CidadeIBGELocal = ReadNullableString(reader, "CidadeIBGELocal") ?? string.Empty,
            CEPLocal = ReadNullableString(reader, "CEPLocal") ?? string.Empty,
            FormaPagto = ReadNullableString(reader, "FormaPagto") ?? string.Empty,
            CondPagto = ReadNullableString(reader, "CondPagto") ?? string.Empty,
            HashPagamento = ReadNullableString(reader, "HashPagamento") ?? string.Empty,
            NmSolicitante = ReadNullableString(reader, "NmSolicitante") ?? string.Empty,
            EmailSolicitante = ReadNullableString(reader, "EmailSolicitante") ?? string.Empty,
            TransportadoraID = ReadNullableInt32(reader, "TransportadoraID") ?? 0,
            NmTransportadora = ReadNullableString(reader, "NmTransportadora") ?? string.Empty,
            CNPJTransportadora = ReadNullableString(reader, "CNPJTransportadora") ?? string.Empty,
            VlrFreteCalc = ReadNullableDecimal(reader, "VlrFreteCalc") ?? 0,
            PrazoEntregaCalc = ReadNullableInt32(reader, "PrazoEntregaCalc") ?? 0,
            PrazoEntregaTransp = ReadNullableInt32(reader, "PrazoEntregaTransp") ?? 0,
            DtProgLiberacao = ReadNullableDateTime(reader, "DtProgLiberacao"),
            DtProgEmbarque = ReadNullableDateTime(reader, "DtProgEmbarque"),
            DtProgEntrega = ReadNullableDateTime(reader, "DtProgEntrega"),
            DtPlanejadaOperacao = ReadNullableDateTime(reader, "DtPlanejadaOperacao"),
            DtSLACliente = ReadNullableDateTime(reader, "DtSLACliente"),
            DtProgEmbFollow = ReadNullableDateTime(reader, "DtProgEmbFollow"),
            FreteAgrupado = ReadNullableString(reader, "FreteAgrupado") ?? string.Empty,
            ObsCalcFrete = ReadNullableString(reader, "ObsCalcFrete") ?? string.Empty,
            DtPrevEntFollow = ReadNullableDateTime(reader, "DtPrevEntFollow"),
            DtPrevisaoEntrega = ReadNullableDateTime(reader, "DtPrevisaoEntrega"),
            StatusSLA = ReadNullableString(reader, "StatusSLA") ?? string.Empty,
            ObsCotacao = ReadNullableString(reader, "ObsCotacao") ?? string.Empty,
            ObsAprovacao = ReadNullableString(reader, "ObsAprovacao") ?? string.Empty,
            ObsNota = ReadNullableString(reader, "ObsNota") ?? string.Empty,
            ObsLocalEntrega = ReadNullableString(reader, "ObsLocalEntrega") ?? string.Empty,
            QtItensBRSupply = ReadNullableInt32(reader, "QtItensBRSupply") ?? 0,
            QtItensTerceiros = ReadNullableInt32(reader, "QtItensTerceiros") ?? 0,
            QtItensRuptura= ReadNullableInt32(reader, "QtItensRuptura") ?? 0,
            ValorItensBRSupply = ReadNullableDecimal(reader, "ValorItensBRSupply") ?? 0,
            ValorItensTerceiros = ReadNullableDecimal(reader, "ValorItensTerceiros") ?? 0,
            VlrFrete = ReadNullableDecimal(reader, "VlrFrete") ?? 0,
            VlrTaxaServico = ReadNullableDecimal(reader, "VlrTaxaServico") ?? 0,
            FlagIntegradoSAP = ReadNullableInt32(reader, "FlagIntegradoSAP") ?? 0
        };
    }

    public async Task<PurchaseOrderSearchResult> SearchByPurchaseOrderAsync(string ordemCompra, CancellationToken cancellationToken = default)
    {
        const string sqlCount = """
            SELECT COUNT(1)
            FROM BR_Cotacao C WITH (NOLOCK)
            WHERE C.OrdemCompra LIKE @ordemCompraLike;
            """;

        const string sqlDetails = """
            WITH TopPedidos AS (
                SELECT TOP 100
                    C.CotacaoID,
                    C.ClienteID,
                    C.OrdemCompra,
                    C.dtCotacao,
                    C.StatusCotacao,
                    C.EstabelecimentoID
                FROM BR_Cotacao C WITH (NOLOCK)
                WHERE C.OrdemCompra LIKE @ordemCompraLike
                ORDER BY C.CotacaoID DESC
            )
            SELECT
                TP.CotacaoID AS Pedido,
                ISNULL(CL.NmCliente, '') AS NmCliente,
                TP.OrdemCompra,
                TP.dtCotacao AS DataPedido,
                ISNULL(S.DsStatusCotacao, '') AS Situacao,
                ISNULL(TotalItens.ValorTotalProdutos, 0) AS VlrTotalProdutos,
                ISNULL(E.NmCurto, '') AS NmEstabelecimento
            FROM TopPedidos TP
            INNER JOIN BR_Cliente CL WITH (NOLOCK) ON CL.ClienteID = TP.ClienteID
            INNER JOIN BR_StatusCotacao S WITH (NOLOCK) ON S.StatusCotacao = TP.StatusCotacao
            INNER JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = TP.EstabelecimentoID
            OUTER APPLY (
                SELECT SUM(ISNULL(T.QtItem, 0) * ISNULL(T.VlrFinal, 0)) AS ValorTotalProdutos
                FROM BR_CotacaoItem T WITH (NOLOCK)
                WHERE T.CotacaoID = TP.CotacaoID
            ) AS TotalItens
            ORDER BY TP.CotacaoID DESC;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var ordemCompraLike = $"%{ordemCompra}%";

        var total = 0;
        await using (var countCmd = new SqlCommand(sqlCount, connection))
        {
            countCmd.Parameters.Add("@ordemCompraLike", SqlDbType.VarChar, 255).Value = ordemCompraLike;
            total = (int)(await countCmd.ExecuteScalarAsync(cancellationToken) ?? 0);
        }

        if (total <= 0)
        {
            return new PurchaseOrderSearchResult
            {
                Total = 0,
                Orders = []
            };
        }

        await using var cmd = new SqlCommand(sqlDetails, connection);
        cmd.Parameters.Add("@ordemCompraLike", SqlDbType.VarChar, 255).Value = ordemCompraLike;

        var orders = new List<PurchaseOrderOrderItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new PurchaseOrderOrderItem
            {
                PedidoId = reader.GetInt32(reader.GetOrdinal("Pedido")),
                ClienteNome = reader.GetString(reader.GetOrdinal("NmCliente")),
                DataPedido = ReadNullableDateTime(reader, "DataPedido"),
                Situacao = reader.GetString(reader.GetOrdinal("Situacao")),
                OrdemCompra = reader.GetString(reader.GetOrdinal("OrdemCompra")),
                ValorTotalProdutos = reader.GetDecimal(reader.GetOrdinal("VlrTotalProdutos")),
                EstabelecimentoNome = reader.GetString(reader.GetOrdinal("NmEstabelecimento"))
            });
        }

        return new PurchaseOrderSearchResult
        {
            Total = total,
            Orders = orders
        };
    }

    public async Task<int?> GetOrderIdByInvoiceAsync(string notaFiscal, int serie, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 CotacaoID AS Pedido
            FROM tssprod..BR_NotaFiscal WITH (NOLOCK)
            WHERE NrNotaFiscal = @notaFiscal
              AND Serie = @serie;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@notaFiscal", SqlDbType.VarChar, 50).Value = notaFiscal;
        cmd.Parameters.Add("@serie", SqlDbType.Int).Value = serie;

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result is null || result == DBNull.Value)
        {
            return null;
        }

        var pedidoId = Convert.ToInt32(result);
        return pedidoId > 0 ? pedidoId : null;
    }

    private static DateTime? ReadNullableDateTime(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static string? ReadNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        //return reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal).ToString();
    }

    private static int? ReadNullableInt32(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static decimal? ReadNullableDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }
}
