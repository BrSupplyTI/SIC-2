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
            DECLARE @QtNotasFiscais INT = 0
            DECLARE @QtRomaneios INT = 0                        
            DECLARE @QtChamados INT = 0                        
            DECLARE @QtAnaliseCredito INT = 0

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
            	ISNULL((SELECT TOP 1 1
            	         FROM Integracao_Clientes..BR_SAP_Pedidos WITH (NOLOCK)
            	         WHERE CotacaoID = @Pedido),0);

            SELECT @QtNotasFiscais = COUNT(*)
            FROM tssprod..BR_NotaFiscal N WITH (NOLOCK)
            WHERE ISNULL(N.CotacaoID,0) = @Pedido
            
            SELECT @QtRomaneios = COUNT(*)
            FROM BR_RomaneioNota N WITH (NOLOCK)
            WHERE N.CotacaoID = @Pedido

            SELECT @QtAnaliseCredito = COUNT(*)  
            FROM BR_CotacaoCredito C WITH (NOLOCK)
            WHERE C.CotacaoID = @Pedido

            SELECT @QtChamados = @QtChamados + COUNT(*) 
            FROM BrWeb..HelpDesk_Chamado C (NOLOCK) 
            WHERE C.NmCampo = 'Número do Pedido'
                AND LTRIM(RTRIM(C.VlrCampo)) = CONVERT(VARCHAR(10), @Pedido)
            SELECT @QtChamados = @QtChamados + COUNT(*) 
            FROM BrWeb..HelpDesk_Chamado C (NOLOCK)
            WHERE C.NmCampo = 'Número da Nota Fiscal'
                AND CHARINDEX('-',LTRIM(RTRIM(ISNULL(C.VlrCampo,'')))) > 0
                AND (LTRIM(RTRIM(ISNULL(C.VlrCampo,'')))) IN (
                    SELECT Z.NrNotaFiscal + '-' + Z.Serie
                    FROM tssprod..BR_NotaFiscal Z (NOLOCK)                        
                    WHERE Z.CotacaoID =  CONVERT(VARCHAR(10), @Pedido))

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
                   @FlagIntegradoSAP AS FlagIntegradoSAP,
                   @QtNotasFiscais AS QtNotasFiscais,
                   @QtRomaneios AS QtRomaneios,
                   @QtChamados AS QtChamados,
                   @QtAnaliseCredito AS QtAnaliseCredito
            FROM BR_Cotacao C WITH (NOLOCK)
            JOIN BR_Estabelecimento W WITH (NOLOCK) ON W.EstabelecimentoID = C.EstabelecimentoID
            JOIN BR_Cliente I WITH (NOLOCK) ON I.ClienteID = C.ClienteID
            JOIN BR_CanalVenda A WITH (NOLOCK) ON A.CanalVendaID = C.CanalVendaID
            JOIN BR_StatusCotacao S WITH (NOLOCK) ON S.StatusCotacao = C.StatusCotacao
            JOIN BR_ClienteEndereco E WITH (NOLOCK) ON E.ClienteEnderecoID = C.ClienteEnderecoID
            JOIN BR_ClienteLocalEntrega L WITH (NOLOCK) ON L.ClienteLocalEntregaID = C.ClienteLocalEntregaID
            LEFT JOIN BR_Cidade CIDE WITH (NOLOCK) ON CIDE.CidadeID = E.CdCidadeEnderecoID
            LEFT JOIN BR_UF UFE WITH (NOLOCK) ON UFE.UFID = CIDE.UFID
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
            FlagIntegradoSAP = ReadNullableInt32(reader, "FlagIntegradoSAP") ?? 0,
            QtNotasFiscais = ReadNullableInt32(reader, "QtNotasFiscais") ?? 0,
            QtRomaneios = ReadNullableInt32(reader, "QtRomaneios") ?? 0,
            QtChamados = ReadNullableInt32(reader, "QtChamados") ??0,
            QtAnaliseCredito = ReadNullableInt32(reader, "QtAnaliseCredito") ?? 0
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

    public async Task<IReadOnlyList<OrderSapIntegrationItem>> GetOrderSapIntegrationAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT NrPedCli,
                   OrdemVenda,
                   MsgRetorno,
                   DtHrEnvioSAP,
                   RemessaSAP,
                   FaturaSAP,
                   NrNF,
                   NumeroNFDanfe,
                   TipoOVSAP,
                   AcaoContorno
            FROM dbo.SIC_Consulta_IntegracaoSAP (@Pedido);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderSapIntegrationItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderSapIntegrationItem
            {
                NrPedCli = ReadStringValue(reader, "NrPedCli"),
                OrdemVenda = ReadStringValue(reader, "OrdemVenda"),
                MsgRetorno = ReadStringValue(reader, "MsgRetorno"),
                DtHrEnvioSAP = ReadStringValue(reader, "DtHrEnvioSAP"),
                RemessaSAP = ReadStringValue(reader, "RemessaSAP"),
                FaturaSAP = ReadStringValue(reader, "FaturaSAP"),
                NrNF = ReadStringValue(reader, "NrNF"),
                TipoOVSAP = ReadStringValue(reader, "TipoOVSAP")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderTaxItem>> GetOrderTaxesAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CI.MVA,
                   CI.VlrTotalNF,
                   CI.itemDocumentoSAP,
                   I.CdItem,
                   CI.MKUP,
                   CI.VlrUnitario,
                   CI.VlrCustoAquisicao,
                   CI.MargemEnviada,
                   CI.PercentualICMS,
                   CI.PercentualFCP,
                   CI.PercentualIPI,
                   CI.PercentualCOFINS,
                   CI.PercentualPIS,
                   CI.ValorICMS,
                   CI.ValorIPI,
                   CI.ValorST,
                   CI.ValorISS,
                   CI.ValorISSRetido,
                   CI.ValorCOFINS,
                   CI.ValorPIS,
                   CI.ValorFCPST,
                   CI.ValorICMSPartilhaOrigem,
                   CI.ValorICMSPartilhaDestino,
                   CI.ValorFundoCombPobreza,
                   CI.ValorPISRetido,
                   CI.ValorCOFINSRetido,
                   CI.ValorCSLRetido,
                   CI.ValorIRRetido,
                   CI.MargemCalculada,
                   CI.LB,
                   CI.ROL
            FROM BrSupply.dbo.BR_CotacaoItem CI
            JOIN BrSupply.dbo.BR_Item I ON I.ItemID = CI.ItemID
            WHERE CI.CotacaoID = @Pedido
            ORDER BY CI.ItemDocumentoSAP DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderTaxItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderTaxItem
            {
                MVA = ReadNullableDecimal(reader, "MVA"),
                VlrTotalNF = ReadNullableDecimal(reader, "VlrTotalNF"),
                ItemDocumentoSAP = ReadStringValue(reader, "itemDocumentoSAP"),
                CdItem = ReadStringValue(reader, "CdItem"),
                MKUP = ReadNullableDecimal(reader, "MKUP"),
                VlrUnitario = ReadNullableDecimal(reader, "VlrUnitario"),
                VlrCustoAquisicao = ReadNullableDecimal(reader, "VlrCustoAquisicao"),
                MargemEnviada = ReadNullableDecimal(reader, "MargemEnviada"),
                PercentualICMS = ReadNullableDecimal(reader, "PercentualICMS"),
                PercentualFCP = ReadNullableDecimal(reader, "PercentualFCP"),
                PercentualIPI = ReadNullableDecimal(reader, "PercentualIPI"),
                PercentualCOFINS = ReadNullableDecimal(reader, "PercentualCOFINS"),
                PercentualPIS = ReadNullableDecimal(reader, "PercentualPIS"),
                ValorICMS = ReadNullableDecimal(reader, "ValorICMS"),
                ValorIPI = ReadNullableDecimal(reader, "ValorIPI"),
                ValorST = ReadNullableDecimal(reader, "ValorST"),
                ValorISS = ReadNullableDecimal(reader, "ValorISS"),
                ValorISSRetido = ReadNullableDecimal(reader, "ValorISSRetido"),
                ValorCOFINS = ReadNullableDecimal(reader, "ValorCOFINS"),
                ValorPIS = ReadNullableDecimal(reader, "ValorPIS"),
                ValorFCPST = ReadNullableDecimal(reader, "ValorFCPST"),
                ValorICMSPartilhaOrigem = ReadNullableDecimal(reader, "ValorICMSPartilhaOrigem"),
                ValorICMSPartilhaDestino = ReadNullableDecimal(reader, "ValorICMSPartilhaDestino"),
                ValorFundoCombPobreza = ReadNullableDecimal(reader, "ValorFundoCombPobreza"),
                ValorPISRetido = ReadNullableDecimal(reader, "ValorPISRetido"),
                ValorCOFINSRetido = ReadNullableDecimal(reader, "ValorCOFINSRetido"),
                ValorCSLRetido = ReadNullableDecimal(reader, "ValorCSLRetido"),
                ValorIRRetido = ReadNullableDecimal(reader, "ValorIRRetido"),
                MargemCalculada = ReadNullableDecimal(reader, "MargemCalculada"),
                LB = ReadNullableDecimal(reader, "LB"),
                ROL = ReadNullableDecimal(reader, "ROL")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<FreightCalculationItem>> GetFreightCalculationHistoryAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT L.TransportadoraID,
                   L.NomeTransportadora,
                   L.PrazoLogistico,
                   L.PrazoComercial,
                   L.TaxaExtra,
                   L.QtItensRestritos,
                   L.FlagClienteRestrito,
                   L.FlagClienteFixo,
                   L.FlagObrigatoriaCanalVenda,
                   L.ValorFrete
            FROM BR_LogisticaCalculoFrete L WITH (NOLOCK)
            WHERE L.CotacaoID = @Pedido
            ORDER BY L.ValorFrete
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<FreightCalculationItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new FreightCalculationItem
            {
                TransportadoraID = ReadNullableInt32(reader, "TransportadoraID") ?? 0,
                NomeTransportadora = ReadStringValue(reader, "NomeTransportadora"),
                PrazoLogistico = ReadNullableInt32(reader, "PrazoLogistico") ?? 0,
                PrazoComercial = ReadNullableInt32(reader, "PrazoComercial") ?? 0,
                TaxaExtra = ReadNullableDecimal(reader, "TaxaExtra") ?? 0,
                QtItensRestritos = ReadNullableInt32(reader, "QtItensRestritos") ?? 0,
                FlagClienteRestrito = ReadNullableInt32(reader, "FlagClienteRestrito") ?? 0,
                FlagClienteFixo = ReadNullableInt32(reader, "FlagClienteFixo") ?? 0,
                FlagObrigatoriaCanalVenda = ReadNullableInt32(reader, "FlagObrigatoriaCanalVenda") ?? 0,
                ValorFrete = ReadNullableDecimal(reader, "ValorFrete") ?? 0
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<FreightCalculationItem>> GetFreightCalculationAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT L.TransportadoraID,
                   L.Nome AS NomeTransportadora,
                   L.PrazoLogistico,
                   L.PrazoComercial,
                   L.TaxaExtra,      
                   L.QtItensRestritos,
                   L.FlagClienteRestrito,
                   L.FlagClienteFixo,
                   L.FlagObrigatoriaCanalVenda,
                   L.ValorFrete
            FROM Fn_Calcula_Fretes_Pedido(@Pedido) L
            ORDER BY L.ValorFrete
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<FreightCalculationItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new FreightCalculationItem
            {
                TransportadoraID = ReadNullableInt32(reader, "TransportadoraID") ?? 0,
                NomeTransportadora = ReadStringValue(reader, "NomeTransportadora"),
                PrazoLogistico = ReadNullableInt32(reader, "PrazoLogistico") ?? 0,
                PrazoComercial = ReadNullableInt32(reader, "PrazoComercial") ?? 0,
                TaxaExtra = ReadNullableDecimal(reader, "TaxaExtra") ?? 0,
                QtItensRestritos = ReadNullableInt32(reader, "QtItensRestritos") ?? 0,
                FlagClienteRestrito = ReadNullableInt32(reader, "FlagClienteRestrito") ?? 0,
                FlagClienteFixo = ReadNullableInt32(reader, "FlagClienteFixo") ?? 0,
                FlagObrigatoriaCanalVenda = ReadNullableInt32(reader, "FlagObrigatoriaCanalVenda") ?? 0,
                ValorFrete = ReadNullableDecimal(reader, "ValorFrete") ?? 0
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderBrSupplyItem>> GetOrderBrSupplyItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CT.ClienteID,
                   I.ItemID,
                   I.CdItem,
                   I.NmItem,
                   CONVERT(INT, C.QtItem) AS QtItem,
                   C.VlrFinal,
                   C.QtItem * C.VlrFinal AS VlrTotal,
                   CASE WHEN C.VlrOriginal > C.VlrFinal
                        THEN VlrOriginal
                        ELSE 0
                   END AS VlrOriginal,
                   ISNULL(C.OrdemCliente, '') + ' / ' + ISNULL(SequenciaCliente,'') AS OrdemCliente,
                   CASE ISNULL(C.FlagAlocaPedido,0)
                        WHEN 0 THEN 'Não Alocado'
                        WHEN 1 THEN 'Alocado'
                        WHEN 2 THEN 'Atendido'
                   END AS SituacaoItem,
                   C.DtAlocacao,
                   C.MargemCalculada,
                   CASE ISNULL(C.FlagArmazem,9)
                        WHEN 1 THEN 'P4'
                        WHEN 0 THEN 'P1'
                        ELSE IIF((SELECT COUNT(*)
                                 FROM BR_ClienteEstoqueTerceiroItem TI WITH (NOLOCK)
                                 JOIN BR_ClienteEstoqueTerceiro T WITH (NOLOCK) ON T.ClienteEstoqueTerceiroID = TI.ClienteEstoqueTerceiroID
                                 WHERE TI.ItemID = I.ItemID
                                   AND T.ClienteID = CT.ClienteID) > 0, 'P4', 'P1')
                   END AS Versao
            FROM BR_CotacaoItem C WITH (NOLOCK)
            JOIN BR_Cotacao CT WITH (NOLOCK) ON CT.CotacaoID = C.CotacaoID
            JOIN BR_Item I (NOLOCK) ON I.ItemID = C.ItemID
            AND C.CotacaoID = @Pedido
            ORDER BY ISNULL(C.FlagAlocaPedido,0),
                     I.NmItem
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderBrSupplyItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderBrSupplyItem
            {
                ClienteID = ReadNullableInt32(reader, "ClienteID") ?? 0,
                ItemID = ReadNullableInt32(reader, "ItemID") ?? 0,
                CdItem = ReadStringValue(reader, "CdItem"),
                NmItem = ReadStringValue(reader, "NmItem"),
                QtItem = ReadNullableInt32(reader, "QtItem") ?? 0,
                VlrFinal = ReadNullableDecimal(reader, "VlrFinal") ?? 0,
                VlrTotal = ReadNullableDecimal(reader, "VlrTotal") ?? 0,
                VlrOriginal = ReadNullableDecimal(reader, "VlrOriginal") ?? 0,
                OrdemCliente = ReadStringValue(reader, "OrdemCliente"),
                SituacaoItem = ReadStringValue(reader, "SituacaoItem"),
                DtAlocacao = ReadNullableDateTime(reader, "DtAlocacao"),
                MargemCalculada = ReadNullableDecimal(reader, "MargemCalculada"),
                Versao = ReadStringValue(reader, "Versao")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderBrSupplyItem>> GetOrderMarketplaceItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CT.ClienteID,
                   I.ItemFornecedorID AS ItemID,
                   I.CdItem,
                   I.NmItem,
                   CONVERT(INT, C.QtItem) AS QtItem,
                   C.VlrFinal,
                   C.QtItem * C.VlrFinal AS VlrTotal,
                   CASE WHEN C.VlrOriginal > C.VlrFinal
                        THEN VlrOriginal
                        ELSE 0
                   END AS VlrOriginal,
                   ISNULL(C.OrdemCliente, '') + ' / ' + ISNULL(SequenciaCliente,'') AS OrdemCliente,
                   I.PathFoto,
                   F.NmFornecedor
            FROM BR_CotacaoItem C WITH (NOLOCK)
            JOIN BR_Cotacao CT WITH (NOLOCK) ON CT.CotacaoID = C.CotacaoID
            JOIN BR_ItemFornecedor I (NOLOCK) ON I.ItemFornecedorID = C.ItemFornecedorID
            JOIN BR_Fornecedor F (NOLOCK) ON F.FornecedorID = I.FornecedorID
            AND C.CotacaoID = @Pedido
            ORDER BY I.NmItem
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderBrSupplyItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderBrSupplyItem
            {
                ClienteID = ReadNullableInt32(reader, "ClienteID") ?? 0,
                ItemID = ReadNullableInt32(reader, "ItemID") ?? 0,
                CdItem = ReadStringValue(reader, "CdItem"),
                NmItem = ReadStringValue(reader, "NmItem"),
                QtItem = ReadNullableInt32(reader, "QtItem") ?? 0,
                VlrFinal = ReadNullableDecimal(reader, "VlrFinal") ?? 0,
                VlrTotal = ReadNullableDecimal(reader, "VlrTotal") ?? 0,
                VlrOriginal = ReadNullableDecimal(reader, "VlrOriginal") ?? 0,
                OrdemCliente = ReadStringValue(reader, "OrdemCliente"),
                PathFoto = ReadStringValue(reader, "PathFoto"),
                NmFornecedor = ReadStringValue(reader, "NmFornecedor")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderBrSupplyItem>> GetOrderBrSupplyItemsRupturaAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT X.ClienteID,
                   I.ItemID,
                   I.CdItem,
                   I.NmItem,
                   CONVERT(INT, C.QtItem) AS QtItem,
                   C.VlrFinal,
                   C.QtItem * C.VlrFinal AS VlrTotal,
                   ISNULL(C.PathDocumento, '') AS MensagemRuptura,
                   CONVERT(INT, ISNULL(P.QtDispEstoque, 0) - ISNULL(P.QtAlocadaSemOV, 0)) AS QtDisponivel,
                   E.DtPrevEntrega,
                   E.QtItemCompra AS QtItemPrevEntrega
            FROM BR_Cotacao X WITH (NOLOCK)
            JOIN BR_CotacaoItem C WITH (NOLOCK) ON C.CotacaoID = X.CotacaoID
            JOIN BR_Item I WITH (NOLOCK) ON I.ItemID = C.ItemID
            JOIN BR_PrecoEstoque P WITH (NOLOCK) ON P.ItemID = I.ItemID AND P.EstabelecimentoID = X.EstabelecimentoID
            OUTER APPLY
            (
                SELECT TOP (1)
                    IE.DtPrevEntrega,
                    CONVERT(INT, IE.QtItemCompra) AS QtItemCompra
                FROM BR_ItemEntrega IE WITH (NOLOCK)
                WHERE IE.EstabelecimentoID = X.EstabelecimentoID
                  AND IE.ItemID = I.ItemID
                ORDER BY IE.DtPrevEntrega ASC
            ) E
            WHERE X.StatusCotacao NOT IN (1, 2, 4, 9, 17, 18, 19)
              AND ISNULL(C.FlagAlocaPedido, 0) = 0
              AND C.CotacaoID = @Pedido
            ORDER BY I.NmItem
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderBrSupplyItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderBrSupplyItem
            {
                ClienteID = ReadNullableInt32(reader, "ClienteID") ?? 0,
                ItemID = ReadNullableInt32(reader, "ItemID") ?? 0,
                CdItem = ReadStringValue(reader, "CdItem"),
                NmItem = ReadStringValue(reader, "NmItem"),
                QtItem = ReadNullableInt32(reader, "QtItem") ?? 0,
                VlrFinal = ReadNullableDecimal(reader, "VlrFinal") ?? 0,
                VlrTotal = ReadNullableDecimal(reader, "VlrTotal") ?? 0,                
                MensagemRuptura = ReadStringValue(reader, "MensagemRuptura"),
                DtPrevEntrega = ReadNullableDateTime(reader, "DtPrevEntrega"),
                QtDisponivel = ReadNullableInt32(reader, "QtDisponivel") ?? 0,
                QtItemPrevEntrega = ReadNullableInt32(reader, "QtItemPrevEntrega") ?? 0
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderApprovalItem>> GetOrderApprovalItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF (SELECT COUNT(*)
                FROM BR_CotAprovAlcada C (NOLOCK)
                JOIN BR_Alcada A (NOLOCK) ON A.AlcadaId = C.AlcadaID
                WHERE C.CotacaoID = @Pedido    
                  AND A.TpAprovacao = 3
                  AND ISNULL(C.ClienteUsuarioID,0) = 0) = 0
            BEGIN
                SELECT U.NmUsuario,
                        CASE A.StatusAlcada
                            WHEN 1 then 'Pendente'
                            WHEN 2 then 'Aguardando Aprovação'
                            WHEN 3 then 'Aprovado'
                            WHEN 4 then 'Reprovado'
                            WHEN 5 then 'Cancelado'
                        END AS StatusAlcada,
                        CASE A.TpAlcada
                            WHEN 1 then 'Verba'
                            WHEN 2 then 'Pedido'
                            ELSE '-'
                        END AS TipoAlcada,
                        A.StatusAlcada as StatusAlcadaID,
                        A.NrSequencia,
                        A.DtAprovacao
                FROM BR_CotAprovAlcada A (NOLOCK)
                JOIN BR_ClienteUsuario U (NOLOCK) ON U.ClienteUsuarioID = A.ClienteUsuarioID
                WHERE A.CotacaoID = @Pedido
                ORDER BY A.NrSequencia
            END ELSE
            BEGIN
                SELECT U.NmUsuario,
                        CASE A.StatusAlcada
                            WHEN 1 then 'Pendente'
                            WHEN 2 then 'Aguardando Aprovação'
                            WHEN 3 then 'Aprovado'
                            WHEN 4 then 'Reprovado'
                            WHEN 5 then 'Cancelado'
                        END AS StatusAlcada,
                        CASE A.TpAlcada
                            WHEN 1 then 'Verba / Qqr. Aprov.'
                            WHEN 2 then 'Pedido / Qqr. Aprov.'
                            ELSE '-'
                        END AS TipoAlcada,
                        A.StatusAlcada as StatusAlcadaID,
                        A.NrSequencia,
                        A.DtAprovacao
                FROM BR_CotAprovAlcada A (NOLOCK)
                JOIN BR_AlcadaItem I (NOLOCK) ON I.AlcadaId = A.AlcadaID
                JOIN BR_Cotacao C (NOLOCK) ON C.CotacaoID = A.CotacaoID
                JOIN BR_ClienteUsuario U (NOLOCK) ON U.ClienteUsuarioID = I.ClienteUsuarioId
                WHERE ISNULL(A.ClienteUsuarioID,0) = 0        
                  AND A.CotacaoID = @Pedido      
            UNION
                SELECT U.NmUsuario,
                    CASE A.StatusAlcada
                        WHEN 1 then 'Pendente'
                        WHEN 2 then 'Aguardando Aprovação'
                        WHEN 3 then 'Aprovado'
                        WHEN 4 then 'Reprovado'
                        WHEN 5 then 'Cancelado'
                    END AS StatusAlcada,
                    CASE A.TpAlcada
                        WHEN 1 then 'Verba'
                        WHEN 2 then 'Pedido'
                        ELSE '-'
                    END AS TipoAlcada,
                    A.StatusAlcada as StatusAlcadaID,
                    A.NrSequencia,
                    A.DtAprovacao
                FROM BR_CotAprovAlcada A (NOLOCK)
                JOIN BR_ClienteUsuario U (NOLOCK) ON U.ClienteUsuarioID = A.ClienteUsuarioID
                WHERE A.CotacaoID = @Pedido
                ORDER BY A.NrSequencia
            END
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderApprovalItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderApprovalItem
            {
                NmUsuario = ReadStringValue(reader, "NmUsuario"),                
                StatusAlcada = ReadStringValue(reader, "StatusAlcada"),
                TipoAlcada = ReadStringValue(reader, "TipoAlcada"),
                StatusAlcadaID = ReadNullableInt32(reader, "StatusAlcadaID"),                
                NrSequencia = ReadNullableInt32(reader, "NrSequencia"),
                DtAprovacao = ReadNullableDateTime(reader, "DtAprovacao")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderInvoiceItem>> GetOrderInvoiceItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT N.NotaFiscalID,
                   N.NrNotaFiscal,
                   N.Serie,
                   N.Chave,
                   N.Operacao,
                   N.EmitCNPJ,
                   N.DtEmissao,
                   SUBSTRING(N.PedCli,1,2) AS Versao,
                   CONVERT(INT, N.QtdeVolumes) AS QtdeVolumes,
                   ISNULL(N.PesoBruto, 0) AS PesoBruto,
                   N.VlrTotalNF,
                   CASE WHEN ISNULL(N.DsStatus, '') = 'EXTRAVIO TOTAL'
                        THEN 9 
                        ELSE ISNULL(N.Status,'0') 
                   END AS StatusNF,
                   ISNULL(N.MotivoCancelamento, ISNULL(N.DsStatus, '')) AS MotivoCancelamento,
                   ISNULL(N.DsStatus, '') AS DsStatusCancelamento,
                   REPLACE(CONVERT(VARCHAR(25),ISNULL(CONVERT(DECIMAL(25,5),N.CubagemNF * 0.000001),0)),'.',',') AS CubagemNF,
                   ISNULL(A.TipoAtestoID, 0) AS TipoAtestoID,
                   ISNULL(CASE ISNULL(A.TipoAtestoID, 0)
                         WHEN 0 THEN ''
                         WHEN 1 THEN 'Recebimento atestado'
                         WHEN 2 THEN 'Recebimento atestado parcialmente'
                         ELSE 'Recebimento contestado'
                    END + ' por ' + ISNULL(U.NmUsuario,'') + 
                    ' em ' + CONVERT(VARCHAR(16), A.DataHora, 103) + ' ' + CONVERT(VARCHAR(5), A.DataHora, 108),'') AS DsAtestoRecebimento        
            FROM tssprod..BR_NotaFiscal N WITH (NOLOCK)
            LEFT JOIN tssprod..BR_NotaFiscalAtesto A WITH (NOLOCK) ON A.NotaFiscalID = N.NotaFiscalID
            LEFT JOIN BR_ClienteUsuario U WITH (NOLOCK) ON U.ClienteUsuarioID = A.ClienteUsuarioID
            WHERE ISNULL(N.CotacaoID,0) = @Pedido
            ORDER BY N.NrNotaFiscal
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderInvoiceItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderInvoiceItem
            {
                NotaFiscalID = reader.GetInt32(reader.GetOrdinal("NotaFiscalID")),
                NrNotaFiscal = ReadStringValue(reader, "NrNotaFiscal"),
                Serie = ReadStringValue(reader, "Serie"),
                Chave = ReadStringValue(reader, "Chave"),
                Operacao = ReadStringValue(reader, "Operacao"),
                EmitCNPJ = ReadStringValue(reader, "EmitCNPJ"),
                DtEmissao = ReadNullableDateTime(reader, "DtEmissao"),
                Versao = ReadStringValue(reader, "Versao"),
                QtdeVolumes = ReadNullableInt32(reader, "QtdeVolumes") ?? 0,
                PesoBruto = ReadNullableDecimal(reader, "PesoBruto") ?? 0,
                VlrTotalNF = ReadNullableDecimal(reader, "VlrTotalNF") ?? 0,
                StatusNF = ReadStringValue(reader, "StatusNF"),
                MotivoCancelamento = ReadStringValue(reader, "MotivoCancelamento"),
                DsStatusCancelamento = ReadStringValue(reader, "DsStatusCancelamento"),
                CubagemNF = ReadStringValue(reader, "CubagemNF"),
                TipoAtestoID = ReadNullableInt32(reader, "TipoAtestoID") ?? 0,
                DsAtestoRecebimento = ReadStringValue(reader, "DsAtestoRecebimento")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderRomaneioItem>> GetOrderRomaneiosAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT N.NrNotaFiscal,
                   N.Serie,
                   N.RomaneioID,
                   T.NmTransportadora AS Transportadora,
                   N.DtPortaria,
                   N.DtEntrega,
                   N.NmRecebedor,
                   E.CdEstabelecimento,
                   E.NmCurto,
                   ISNULL(H.NmHub,'') AS NmHub,
                   ISNULL(X.NmTipoRomaneio, 'Fracionado') AS NmTipoRomaneio,
                   CASE ISNULL(N.TemComprovante,'NAO')
                         WHEN 'SIM' THEN 1 
                         ELSE 0
                   END AS FlagTemComprovante,                   
                   ISNULL(N.NmArquivoComprovante,'') AS NmArquivoComprovante,
                   IIF(ISNULL(R.FlagPronto,0) = 0, 'Em Embarque',
                        IIF(ISNULL(N.NmRecebedor,'') = '', 'Em Rota',
                              IIF((N.NmRecebedor IN ('EXTRAVIO TOTAL', 'Retorno para Reembarque')), 'Com Ocorrência',
                        'Entregue'))) AS SituacaoRomaneio
            FROM BR_RomaneioNota N WITH (NOLOCK)
            JOIN BR_Romaneio R WITH (NOLOCK) ON R.RomaneioID = N.RomaneioID
            JOIN BR_Transportadora T WITH (NOLOCK) ON T.TransportadoraID = R.TransportadoraID
            JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = R.EstabelecimentoID
            LEFT JOIN BR_RomaneioTipo X WITH (NOLOCK) ON X.TipoRomaneioID = R.TipoRomaneio
            LEFT JOIN BR_RomaneioHub H WITH (NOLOCK) ON H.RomaneioHubID = R.RomaneioHubID
            WHERE N.CotacaoID = @Pedido
            ORDER BY N.RomaneioID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderRomaneioItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderRomaneioItem
            {
                RomaneioID = ReadNullableInt32(reader, "RomaneioID") ?? 0,
                NrNotaFiscal = ReadStringValue(reader, "NrNotaFiscal"),
                Serie = ReadStringValue(reader, "Serie"),
                NmTipoRomaneio = ReadStringValue(reader, "NmTipoRomaneio"),
                CdEstabelecimento = ReadStringValue(reader, "CdEstabelecimento"),
                NmCurto = ReadStringValue(reader, "NmCurto"),
                Transportadora = ReadStringValue(reader, "Transportadora"),
                DtPortaria = ReadNullableDateTime(reader, "DtPortaria"),
                NmRecebedor = ReadStringValue(reader, "NmRecebedor"),
                DtEntrega = ReadNullableDateTime(reader, "DtEntrega"),
                NmHub = ReadStringValue(reader, "NmHub"),
                FlagTemComprovante = ReadNullableInt32(reader, "FlagTemComprovante") ?? 0,
                NmArquivoComprovante = ReadStringValue(reader, "NmArquivoComprovante"),
                SituacaoRomaneio = ReadStringValue(reader, "SituacaoRomaneio")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderTrackingItem>> GetOrderTrackingAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT T.DtEvento,
                   E.NmEvento AS Evento,
                   CASE (ISNULL(T.NrPedcli,''))
                      WHEN '' THEN CONVERT(VARCHAR(12), CotacaoID) + ' - ' + T.Detalhes
                      ELSE T.NrPedcli + ' - ' + T.Detalhes
                   END AS Detalhes,
                   T.Usuario
            FROM BR_Tracking T WITH (NOLOCK)
            JOIN BR_TrackingEvento E WITH (NOLOCK) ON E.TrackingEventoID = T.TrackingEventoID
            WHERE T.CotacaoID = @Pedido
            ORDER BY T.DtEvento DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        var items = new List<OrderTrackingItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderTrackingItem
            {
                DtEvento = ReadNullableDateTime(reader, "DtEvento"),
                Evento = ReadStringValue(reader, "Evento"),
                Detalhes = ReadStringValue(reader, "Detalhes"),
                Usuario = ReadStringValue(reader, "Usuario")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderVolumeColetaItem>> GetVolumesColetaAsync(string pedCli, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_PedidoConsultaVolumesColeta", connection);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@PedCli", SqlDbType.VarChar, 50).Value = pedCli;

        var items = new List<OrderVolumeColetaItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderVolumeColetaItem
            {
                CdItem = ReadStringValue(reader, "CdItem"),
                NmItem = ReadStringValue(reader, "NmItem"),
                QtSolicitada = ReadNullableInt32(reader, "QtSolicitada") ?? 0,
                QtColetada = ReadNullableInt32(reader, "QtColetada") ?? 0,
                Volume = ReadStringValue(reader, "Volume"),
                NumVol = ReadNullableInt32(reader, "NumVol") ?? 0,
                DataColeta = ReadStringValue(reader, "DataColeta"),
                NmOperador = ReadStringValue(reader, "NmOperador"),
                EnderecoAtual = ReadStringValue(reader, "EnderecoAtual"),
                ObsCarga = ReadStringValue(reader, "ObsCarga"),
                DtLeituraRomaneio = ReadStringValue(reader, "DtLeituraRomaneio")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderTicketItem>> GetOrderTicketsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT C.ChamadoID AS Protocolo,
                'Pedido' AS Origem,
                LTRIM(RTRIM(C.VlrCampo)) AS OrigemValor,
                ISNULL(Q.NmUsuario, U.NmUsuario) AS NmSolicitante,
                ISNULL(Q.email, U.Email) AS EmailSolicitante,
                A.NmArea,
                N.NmNivel,
                P.NmProblema,
                S.DsStatus AS Situacao,
                CASE (C.StatusChamadoID)
                    WHEN 1 THEN (CASE WHEN (C.PrazoResolucao < GETDATE()) THEN 'Atrasado' ELSE '' END)
                    WHEN 2 THEN (CASE WHEN (C.PrazoResolucao < GETDATE()) THEN 'Atrasado' ELSE '' END)
                    ELSE ''
                END AS Atraso,
                C.DtHrAbertura,
                C.DtHrEncerramento,
                C.PrazoResolucao
            FROM BrWeb..HelpDesk_Chamado C (NOLOCK)
            JOIN BrWeb..HelpDesk_Problema P (NOLOCK) ON P.ProblemaID = C.ProblemaID
            JOIN BrWeb..HelpDesk_StatusChamado S (NOLOCK) ON S.StatusChamadoID = C.StatusChamadoID
            JOIN BrWeb..HelpDesk_Nivel N (NOLOCK) ON N.NivelID = P.NivelID
            JOIN BrWeb..HelpDesk_Area A (NOLOCK) ON A.AreaID = N.AreaID
            LEFT JOIN BR_Usuario U (NOLOCK) ON U.UsuarioID = C.UsuarioAberturaID
            LEFT JOIN BR_ClienteUsuario Q (NOLOCK) ON Q.ClienteUsuarioID = C.ClienteUsuarioID
            WHERE C.NmCampo = 'Número do Pedido'
              AND LTRIM(RTRIM(C.VlrCampo)) = @Pedido
            UNION ALL
            SELECT C.ChamadoID AS Protocolo,
                'Nota Fiscal' AS Origem,
                LTRIM(RTRIM(ISNULL(C.VlrCampo,''))) AS OrigemValor,
                ISNULL(Q.NmUsuario, U.NmUsuario) AS NmSolicitante,
                ISNULL(Q.email, U.Email) AS EmailSolicitante,
                A.NmArea,
                N.NmNivel,
                P.NmProblema,
                S.DsStatus AS Situacao,
                CASE (C.StatusChamadoID)
                    WHEN 1 THEN (CASE WHEN (C.PrazoResolucao < GETDATE()) THEN 'Atrasado' ELSE '' END)
                    WHEN 2 THEN (CASE WHEN (C.PrazoResolucao < GETDATE()) THEN 'Atrasado' ELSE '' END)
                    ELSE ''
                END AS Atraso,
                C.DtHrAbertura,
                C.DtHrEncerramento,
                C.PrazoResolucao
            FROM BrWeb..HelpDesk_Chamado C (NOLOCK)
            JOIN BrWeb..HelpDesk_Problema P (NOLOCK) ON P.ProblemaID = C.ProblemaID
            JOIN BrWeb..HelpDesk_StatusChamado S (NOLOCK) ON S.StatusChamadoID = C.StatusChamadoID
            JOIN BrWeb..HelpDesk_Nivel N (NOLOCK) ON N.NivelID = P.NivelID
            JOIN BrWeb..HelpDesk_Area A (NOLOCK) ON A.AreaID = N.AreaID
            LEFT JOIN BR_Usuario U (NOLOCK) ON U.UsuarioID = C.UsuarioAberturaID
            LEFT JOIN BR_ClienteUsuario Q (NOLOCK) ON Q.ClienteUsuarioID = C.ClienteUsuarioID
            WHERE C.NmCampo = 'Número da Nota Fiscal'
              AND CHARINDEX('-',LTRIM(RTRIM(ISNULL(C.VlrCampo,'')))) > 0
              AND (LTRIM(RTRIM(ISNULL(C.VlrCampo,'')))) IN (
                    SELECT Z.NrNotaFiscal + '-' + Z.Serie
                    FROM tssprod..BR_NotaFiscal Z (NOLOCK)
                    WHERE Z.CotacaoID = @Pedido)
            ORDER BY PrazoResolucao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Pedido", SqlDbType.VarChar, 20).Value = pedido.ToString();

        var items = new List<OrderTicketItem>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderTicketItem
            {
                Protocolo = ReadNullableInt32(reader, "Protocolo") ?? 0,
                Origem = ReadStringValue(reader, "Origem"),
                OrigemValor = ReadStringValue(reader, "OrigemValor"),
                NmSolicitante = ReadStringValue(reader, "NmSolicitante"),
                EmailSolicitante = ReadStringValue(reader, "EmailSolicitante"),
                NmArea = ReadStringValue(reader, "NmArea"),
                NmNivel = ReadStringValue(reader, "NmNivel"),
                NmProblema = ReadStringValue(reader, "NmProblema"),
                Situacao = ReadStringValue(reader, "Situacao"),
                Atraso = ReadStringValue(reader, "Atraso"),
                DtHrAbertura = ReadNullableDateTime(reader, "DtHrAbertura"),
                DtHrEncerramento = ReadNullableDateTime(reader, "DtHrEncerramento"),
                PrazoResolucao = ReadNullableDateTime(reader, "PrazoResolucao")
            });
        }

        return items;
    }

    public async Task<OrderCreditAnalysis?> GetOrderCreditAnalysisAsync(int pedido, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 
                   C.MotivoBloqueio,
                   C.FlagAprovado,
                   CASE(C.FlagAprovado)
                        WHEN 0 THEN 'Aguardando Avaliação'
                        WHEN 1 THEN 'Crédito Aprovado'
                        ELSE 'Crédito Reprovado'
                   END AS StatusAprovacao,
                   C.DataHoraBloqueio,
                   U.NmUsuario,
                   C.DataHoraAprovacao,
                   C.MotivoAprovacao
            FROM BR_CotacaoCredito C WITH (NOLOCK)
            LEFT JOIN BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = C.UsuarioAprovador
            WHERE C.CotacaoID = @Pedido
            ORDER BY C.DataHoraBloqueio
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

        return new OrderCreditAnalysis
        {
            MotivoBloqueio = ReadStringValue(reader, "MotivoBloqueio"),
            FlagAprovado = ReadNullableInt32(reader, "FlagAprovado"),
            StatusAprovacao = ReadStringValue(reader, "StatusAprovacao"),
            DataHoraBloqueio = ReadNullableDateTime(reader, "DataHoraBloqueio"),
            NmUsuario = ReadStringValue(reader, "NmUsuario"),
            DataHoraAprovacao = ReadNullableDateTime(reader, "DataHoraAprovacao"),
            MotivoAprovacao = ReadStringValue(reader, "MotivoAprovacao")
        };
    }

    public async Task<IReadOnlyList<OrderValidationItem>> GetOrderValidationsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_ConsultaPedidoValicacao", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<OrderValidationItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderValidationItem
            {
                Erro = ReadStringValue(reader, "Erro"),
                Correcao = ReadStringValue(reader, "Correcao")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<OrderLogItem>> GetOrderLogsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_PedidoConsultaLogs", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add("@Pedido", SqlDbType.Int).Value = pedido;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<OrderLogItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderLogItem
            {
                Origem = ReadStringValue(reader, "Origem"),
                DataHora = ReadNullableDateTime(reader, "DataHora"),
                Acao = ReadStringValue(reader, "Acao"),
                Descricao = ReadStringValue(reader, "Descricao"),
                NmUsuario = ReadStringValue(reader, "NmUsuario")
            });
        }

        return items;
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

    public async Task<string?> GetInvoiceXmlAsync(string chaveDanfe, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(XMLData AS NVARCHAR(MAX))
            FROM tssprod..BR_NotaFiscalXML WITH (NOLOCK)
            WHERE ChaveDanfe = @chave
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@chave", chaveDanfe);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result as string;
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

    private static string ReadStringValue(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal)) ?? string.Empty;
    }
}
