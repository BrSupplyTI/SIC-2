using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.Cotacao;
using SIC.Domain.Entities.Cotacao;

namespace SIC.Infrastructure.Repositories.Cotacao;

/// <summary>
/// Implementação SQL das operações de leitura da Cotação.
/// </summary>
public sealed class SqlCotacaoQueryRepository(IConfiguration configuration) : ICotacaoQueryRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    private const string BuscarCatalogoSql = """
        SET NOCOUNT ON
        DECLARE @Tbl TABLE
        (
            ItemID INT,
            FlagTipo INT,
            Prioridade INT,
            Probabilidade INT,
            CdItem VARCHAR(100),
            NmItem VARCHAR(1000),
            NmFornecedor VARCHAR(100),
            ProdutoMarcaID INT,
            Marca VARCHAR(1000),
            Premium INT,
            Standard INT,
            Basic INT,
            VlrTabela DECIMAL(18,2)
        )
        INSERT @Tbl
        EXEC BrSupply.dbo.BRS_sp_PesquisaCatalogo_V2 @Descricao, 0, @ClienteID, @TblPrecoID, 0, 1, 0, 1, 200, 1, 0
        SELECT
            T.ItemID AS ItemID
            , I.CdItem AS CdItem
            , I.NmItem AS NmItem
            , S.SegmentoID AS SegmentoID
            , S.NmSegmento AS NmSegmento
            , F.FamiliaID AS FamiliaID
            , F.NmFamilia AS NmFamilia
            , SF.SubFamiliaID AS SubFamiliaID
            , SF.NmSubFamilia AS NmSubFamilia
            , PE.EstabelecimentoID AS EstabelecimentoID
            , ISNULL(PE.Curva,'') AS Curva
            , CAST((ISNULL(PE.QtDispEstoque,0) - ISNULL(PE.QtAlocadaSemOV,0)) AS INT) AS QtdDisponivel
            , CONVERT(INT,(ISNULL(PE.QtDispEstoque,0) - ISNULL(PE.QtAlocadaSemOV,0))) AS QtEstoqueSIC
            , IIF(ISNULL(I.FlagAtivo,0) = 1, 'SIM', 'NÃO') AS Ativo
            , ISNULL(PE.VlrCustoAquisicao, 0) AS VlrCustoAquisicao
            , ISNULL(PE.VlrCustoMedio, 0) AS VlrCustoMedio
            , COALESCE(
                NULLIF(T.VlrTabela, 0),
                NULLIF(PE.VlrCustoAquisicao, 0),
                PE.VlrCustoMedio,
                0
            ) AS VlrTabela
            , CASE
                WHEN ISNULL(PE.FlagOutlet, 0) = 1 THEN 'Y'
                ELSE CASE
                    WHEN ISNULL(I.FlagSobDemanda, 0) = 1 THEN 'Z'
                    ELSE 'X'
                END
            END AS Criticidade
            , FORMAT(ISNULL(T.VlrTabela, 0), 'N', 'pt-br') AS TabelaPreco
            , COALESCE(
                NULLIF((
                    SELECT TOP 1 COALESCE(TPI.VlrUnitMinimo, TPI.VlrUnit, 0)
                    FROM BrSupply.dbo.BR_TblPrecoItem TPI WITH (NOLOCK)
                    INNER JOIN BrSupply.dbo.BR_TblPrecoVig TPV WITH (NOLOCK) ON TPV.TblPrecoVigID = TPI.TblPrecoVigID
                    WHERE TPI.ItemID = T.ItemID
                      AND TPV.TblPrecoID = @TblPrecoID
                    ORDER BY TPV.TblPrecoVigID DESC
                ), 0),
                NULLIF(T.VlrTabela, 0),
                NULLIF(PE.VlrCustoAquisicao, 0),
                PE.VlrCustoMedio,
                0
              ) AS VlrPrecoMinimo
        FROM @Tbl T
        INNER JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = T.ItemID
        INNER JOIN BrSupply.dbo.BR_Segmento S (NOLOCK) ON S.SegmentoID = I.SegmentoID
        INNER JOIN BrSupply.dbo.BR_Familia F (NOLOCK) ON F.FamiliaID = I.FamiliaID
        INNER JOIN BrSupply.dbo.BR_SubFamilia SF (NOLOCK) ON SF.SubFamiliaID = I.SubFamiliaID
        INNER JOIN BrSupply.dbo.BR_PrecoEstoque PE (NOLOCK) ON PE.ItemID = I.ItemID
        LEFT JOIN BrSupply.dbo.BR_ProdutoMarca M (NOLOCK) ON M.ProdutoMarcaID = I.ProdutoMarcaID
        WHERE I.FlagAtivo = 1
            AND PE.EstabelecimentoID = @EstabelecimentoID
            AND NOT EXISTS (
                SELECT 1 FROM BrWeb..Proposta_Itens PI
                WHERE PI.PropostaID = @PropostaID AND PI.CodItemBR = T.CdItem
            )
        ORDER BY
            T.Probabilidade DESC
            , ISNULL(S.FlagConsultaProduto,99) ASC
            , I.FlagAtivo DESC
            , ISNULL(M.FlagTipoMarca,'zzz') ASC
            , ISNULL(PE.FlagOutlet, 0) ASC
        """;

    private const string BaseListSql = """
        SET NOCOUNT ON;
        EXEC BrWeb..SIC_ListaPropostas 
            @UsuarioID, 
            @FiltroCotacao, 
            @CdExtCliente,
            @PropostaID, 
            @CNPJ, 
            @EstabelecimentoID, 
            @StatusID, 
            @DataInicial, 
            @DataFinal
        """;

    private const string DetalheSql = """
        SELECT 
            Proposta.PropostaId AS PropostaID,
            Proposta.CdProposta AS CdProposta,
            Proposta.Nome AS Nome,
            Proposta.Versao AS Versao,
            Proposta.OrdemCompra AS OrdemCompra,
            Proposta.StatusID AS StatusID,
            Status.NmStatus AS StatusNome,
            Proposta.TipoCotacao AS TipoCotacao,
            CONVERT(VARCHAR(10), Proposta.DataValidade, 103) AS DataValidade,
            Estabelecimento.EstabelecimentoID AS EstabelecimentoID,
            Estabelecimento.NmEstabelecimento AS EstabelecimentoNome,
            (CASE WHEN Proposta.UfOrigem = 'SP' AND CE.UFID = 1 
                  THEN (SELECT EstabelCNPJVirtual FROM BrSupply.dbo.BR_Estabelecimento WHERE EstabelecimentoID = 9)
                  ELSE Estabelecimento.EstabelCNPJ 
             END) AS EstabelecimentoCNPJ,
            Estabelecimento.EstabelRazaoSocial AS EstabelecimentoRazaoSocial,
            Cliente.ClienteID AS ClienteID,
            Cliente.CdExtCliente AS ClienteCodigo,
            Cliente.NmCliente AS ClienteNome,
            (ISNULL(Cliente.CdExtCliente, '') + ' - ' + ISNULL(Cliente.NmCliente, '')) AS ClienteCodNome,
            Cliente.CNPJCliente AS ClienteCNPJ,
            (CASE Proposta.Contribuinte WHEN 0 THEN 'SIM' ELSE 'NÃO' END) AS ClienteContribuinte,
            (CASE Cliente.InscrEstCliente WHEN 'ISENTO' THEN 0 ELSE 1 END) AS EhContribuinte,
            Proposta.ClienteEnderecoID AS ClienteEnderecoID,
            (ISNULL(CE.Logradouro, '') + ', Nº ' + ISNULL(CE.Numero, '') + ', Bairro: ' + ISNULL(CE.Bairro, '') + ' - CEP: ' + ISNULL(CE.CEP, '') + ' | ' + ISNULL(CE.Cidade, '') + ' - ' + ISNULL(UF.CdUF, '')) AS ClienteEndereco,
            (ISNULL(CE.Cidade, '') + ' - ' + ISNULL(UF.CdUF, '')) AS ClienteCidadeEstado,
            CL.ClienteLocalEntregaID AS ClienteLocalEntregaID,
            CL.NmLocalEntrega AS LocalEntregaNome,
            (ISNULL(CL.DsLogradouro, '') + ', Nº ' + ISNULL(CL.DsNumero, '') + ', Bairro: ' + ISNULL(CL.DsBairro, '') + ' - CEP: ' + ISNULL(CL.DsCEP, '')) AS LocalEntregaEndereco,
            (ISNULL(CI.NmCidade, '') + ' - ' + ISNULL(UFL.CdUF, '')) AS LocalEntregaCidadeEstado,
            ISNULL(CL.ObsLocalEntrega,'') AS LocalEntregaObservacao,
            ISNULL(CV.NmCanalVenda,'') AS CanalVenda,
            ISNULL(TP.Tipo,'') + ' - ' + ISNULL(TP.Descricao,'') AS TipoOrdem,
            Proposta.tipoOVSAP AS TipoOVSAP,
            (SELECT BrWeb.dbo.Fn_NaturezaOperacaoEhRevenda(TipoOV.Tipo)) AS TipoOVEhRevenda,
            Proposta.TipoMotivoIDSAP AS TipoMotivoIDSAP,
            ISNULL(MP.CodTipo + ' - ' + MP.Descricao,'') AS Motivo,
            Motivo.NmMotivo AS MotivoNome,
            Proposta.Justificativa AS Justificativa,
            ISNULL(Proposta.AprovadorUsuarioID, 0) AS AprovadorUsuarioID,
            ISNULL(Aprov.NmUsuario,'') AS AprovadorNome,
            Proposta.JustificativaAprovador AS AprovadorJustificativa,
            Proposta.CondPagto AS CondPagtoID,
            CP.NmCondPagto AS CondPagtoNome,
            Proposta.FormaPagamentoSAP AS FormaPagamentoSAP,
            SFP.Descricao AS FormaPagamentoDesc,
            ISNULL(Proposta.FlagDefCondPagTelevendas,0) AS FlagDefCondPagTelevendas,
            ISNULL(Proposta.TabelaPrecoID,'') AS TabelaPrecoID,
            TP2.NmTblPreco AS TabelaPrecoNome,
            Proposta.FlagPrecoConformeTabela AS FlagPrecoConformeTabela,
            Proposta.MargemPadrao AS MargemPadrao,
            Proposta.MargemBruta AS MargemBruta,
            Proposta.MargemContribuida AS MargemContribuida,
            Proposta.MargemBrutaFixa AS MargemBrutaFixa,
            Proposta.MargemContribuidaFixa AS MargemContribuidaFixa,
            FORMAT(Proposta.Frete, 'C', 'pt-br') AS Frete,
            Proposta.ValorVendaTotal AS ValorVendaTotal,
            Proposta.VlrContribTotal AS VlrContribTotal,
            Proposta.ValorContribuicaoFixo AS ValorContribuicaoFixo,
            Proposta.ValorTotalFixo AS ValorTotalFixo,
            Proposta.VlrPedidoMinimo AS VlrPedidoMinimo,
            (SELECT SUM(PItem.VlrPrecoVenda) FROM BrWeb..Proposta_Itens AS PItem WITH (NOLOCK) WHERE PItem.PropostaID = Proposta.PropostaId) AS TotalVendaNumerico,
            FORMAT((SELECT SUM(PItem.VlrPrecoVenda) FROM BrWeb..Proposta_Itens AS PItem WITH (NOLOCK) WHERE PItem.PropostaID = Proposta.PropostaId), 'C', 'pt-br') AS TotalVenda,
            FORMAT((SELECT (SUM(PItem.VlrPrecoVenda) + ISNULL(Proposta.Frete,0)) FROM BrWeb..Proposta_Itens AS PItem WITH (NOLOCK) WHERE PItem.PropostaID = Proposta.PropostaId), 'C', 'pt-br') AS TotalVendaFrete,
            FORMAT((SELECT SUM(PItem.PrecoItem * PItem.Quantidade) FROM BrWeb..Proposta_Itens AS PItem WITH (NOLOCK) WHERE PItem.PropostaID = Proposta.PropostaId), 'C', 'pt-br') AS TotalVendaSemImposto,
            FORMAT((SELECT (SUM(PItem.PrecoItem * PItem.Quantidade) + ISNULL(Proposta.Frete,0)) FROM BrWeb..Proposta_Itens AS PItem WITH (NOLOCK) WHERE PItem.PropostaID = Proposta.PropostaId), 'C', 'pt-br') AS TotalVendaFreteSemImposto,
            (SELECT SUM(PItem.Peso) FROM BrWeb..Proposta_Itens PItem (NOLOCK) WHERE PItem.PropostaID = Proposta.PropostaId) AS TotalPeso,
            (SELECT COUNT(*) FROM BrWeb..Proposta_Itens AS PropIt (NOLOCK) WHERE PropIt.PropostaID = Proposta.PropostaId) AS QtdItens,
            Proposta.DiasPrazoEntrega AS DiasPrazoEntrega,
            CONVERT(VARCHAR(10), BrSupply.dbo.FN_SomaDiasUteis(GETDATE(), ISNULL(Proposta.DiasPrazoEntrega, 0)), 103) AS DataProgEntrega,
            Proposta.NatOperacao AS NatOperacao,
            Proposta.UfOrigem AS UfOrigem,
            Proposta.UfDestino AS UfDestino,
            Proposta.CodigoIBGE AS CodigoIBGE,
            Proposta.ContatoNome AS ContatoNome,
            Proposta.ContatoEmail AS ContatoEmail,
            Transportadora.TransportadoraID AS TransportadoraID,
            Transportadora.NmTransportadora AS TransportadoraNome,
            Proposta.CotacaoID AS CotacaoID,
            Proposta.CotacaoIdOriginal AS CotacaoIdOriginal,
            CotacaoStatus.DsStatusCotacao AS CotacaoStatusDesc,
            CotacaoEnvio.Comentarios AS CotacaoEnvioComentarios,
            ISNULL(CotacaoEnvio.FlagRevisarValorProdutos,0) AS FlagRevisarValorProdutos,
            ISNULL(CotacaoEnvio.FlagRevisarValorFrete,0) AS FlagRevisarValorFrete,
            ISNULL(PC.FlagRevisarPrazoPagamento,0) AS FlagRevisarPrazoPagamento,
            ISNULL(CotacaoEnvio.FlagRevisarPrazoEntrega,0) AS FlagRevisarPrazoEntrega,
            ISNULL(CotacaoEnvio.FlagRevisarAtendimento,0) AS FlagRevisarAtendimento,
            ISNULL(CotacaoEnvio.FlagRevisarPermiteTrocarMarca,0) AS FlagRevisarPermiteTrocarMarca,
            ISNULL(CotacaoEnvio.FlagRevisarPermiteTrocarUnidade,0) AS FlagRevisarPermiteTrocarUnidade,
            ISNULL(CotacaoEnvio.FlagPrecosInformados,0) AS FlagPrecosInformados,
            ISNULL(CotacaoEnvio.IPAprovacao,'') AS CotacaoEnvioIPAprovacao,
            Consultor.UsuarioID AS ConsultorUsuarioID,
            Consultor.NmUsuario AS ConsultorNome,
            Consultor.Email AS ConsultorEmail,
            Carteira.NmCarteira AS CarteiraNome,
            PropostaVersao.Observacao AS Observacao,
            Proposta.Obs AS Obs
        FROM BrWeb.dbo.Proposta Proposta (NOLOCK)
        LEFT JOIN BrSupply.dbo.BR_Usuario Aprov (NOLOCK) ON Aprov.UsuarioID = Proposta.AprovadorUsuarioID
        LEFT JOIN BrSupply.dbo.BR_Estabelecimento Estabelecimento (NOLOCK) ON Proposta.EstabelecimentoID = Estabelecimento.EstabelecimentoID
        LEFT JOIN BrSupply.dbo.BR_Transportadora Transportadora (NOLOCK) ON Proposta.TransportadoraID = Transportadora.TransportadoraID
        LEFT JOIN BrWeb.dbo.Proposta_Status Status (NOLOCK) ON Proposta.StatusID = Status.StatusID
        LEFT JOIN BrSupply.dbo.BR_Cotacao Cotacao (NOLOCK) ON Proposta.CotacaoID = Cotacao.CotacaoID
        LEFT JOIN BrSupply.dbo.BR_StatusCotacao CotacaoStatus (NOLOCK) ON Cotacao.StatusCotacao = CotacaoStatus.StatusCotacao
        LEFT JOIN Integracao_Clientes..BR_SAP_TiposDocumentosPedidos TipoOV (NOLOCK) ON TipoOV.Tipo = Proposta.tipoOVSAP
        LEFT JOIN BrWeb.dbo.Proposta_Versao PropostaVersao (NOLOCK) ON PropostaVersao.PropostaID = Proposta.PropostaID AND PropostaVersao.Versao = Proposta.Versao
        LEFT JOIN BRWeb.dbo.Proposta_CotacaoEnvio CotacaoEnvio (NOLOCK) ON Proposta.PropostaId = CotacaoEnvio.PropostaID AND CotacaoEnvio.StatusID = 8
        LEFT JOIN BrWeb.dbo.Proposta_CotacaoEnvio PC (NOLOCK) ON PC.PropostaID = Proposta.PropostaId
        LEFT JOIN BrSupply.dbo.BR_Usuario Consultor (NOLOCK) ON Proposta.UsuarioId = Consultor.UsuarioId
        LEFT JOIN BrSupply.dbo.BR_Cliente Cliente (NOLOCK) ON Proposta.ClienteId = Cliente.ClienteID
        LEFT JOIN BrSupply.dbo.BR_Carteira Carteira (NOLOCK) ON Cliente.CarteiraID = Carteira.CarteiraID
        LEFT JOIN BrWeb.dbo.Proposta_Motivo Motivo (NOLOCK) ON Motivo.MotivoID = Proposta.MotivoID
        LEFT JOIN BrSupply.dbo.BR_CondPagto CP (NOLOCK) ON CP.CondPagtoID = Proposta.CondPagto
        LEFT JOIN BrSupply.dbo.BR_TblPreco TP2 (NOLOCK) ON TP2.TblPrecoID = Proposta.TabelaPrecoID
        LEFT JOIN Integracao_Clientes.dbo.BR_SAP_FormasPagamento SFP (NOLOCK) ON SFP.Id = Proposta.FormaPagamentoSAP
        LEFT JOIN BrSupply.dbo.BR_ClienteLocalEntrega CL (NOLOCK) ON CL.ClienteLocalEntregaID = Proposta.ClienteLocalEntregaID
        LEFT JOIN BrSupply.dbo.BR_CanalVenda CV (NOLOCK) ON CV.CanalVendaID = CL.CanalVendaID
        LEFT JOIN BrSupply.dbo.BR_ClienteEndereco CE (NOLOCK) ON CE.ClienteEnderecoID = Proposta.ClienteEnderecoID
        LEFT JOIN BrSupply.dbo.BR_UF UF (NOLOCK) ON CE.UFID = UF.UFID
        LEFT JOIN BrSupply.dbo.BR_Cidade CI (NOLOCK) ON CI.CidadeID = CL.CdCidadeID
        LEFT JOIN BrSupply.dbo.BR_UF UFL (NOLOCK) ON UFL.UFID = CL.CdUFID
        LEFT JOIN Integracao_Clientes..BR_SAP_TiposDocumentosPedidos TP (NOLOCK) ON TP.Tipo = Proposta.tipoOVSAP
        LEFT JOIN Integracao_Clientes..BR_SAP_MotivosPedidos MP (NOLOCK) ON MP.Id = Proposta.TipoMotivoIDSAP
        WHERE Proposta.PropostaId = @PropostaID
        """;

    private const string PropostaItensSql = """
        SELECT ROW_NUMBER() OVER(ORDER BY PropostaItem.PropostaItemID ASC) AS PropostaItem__Numero,
                    PropostaItem.PropostaItemID AS PropostaItem__PropostaItemID,
                    PropostaItem.PropostaID AS PropostaItem__PropostaID,
                    PropostaItem.CodItemBR AS PropostaItem__CodItemBR,
                    PropostaItem.DescrItemBR AS PropostaItem__DescrItemBR,
                    PropostaItem.Target AS PropostaItem__Target,
                    PropostaItem.Quantidade AS PropostaItem__Quantidade,
                    PropostaItem.PrecoItem AS PropostaItem__PrecoItem,
                    PropostaItem.VlrPrecoMargem AS PropostaItem__MargemCalculada,
                    PropostaItem.ICM AS PropostaItem__ICM,
                    PropostaItem.Pis AS PropostaItem__Pis,
                    PropostaItem.ValorPis AS PropostaItem__ValorPis,
                    PropostaItem.UniMedBr AS PropostaItem__UniMedBr,
                    PropostaItem.VlrContribuido AS PropostaItem__VlrContribuido,
                    PropostaItem.VlrCustoAquisicao AS PropostaItem__VlrCustoAquisicao,
                    PropostaItem.VlrCustoMedio AS PropostaItem__VlrCustoMedio,
                    PropostaItem.VlrPrecoVenda AS PropostaItem__VlrPrecoVenda,
                    PropostaItem.Margem AS PropostaItem__Margem,
                    PropostaItem.ValorLiqUnit as PropostaItem__ValorLiqUnit,
                    PropostaItem.ValorICMS as PropostaItem__ValorICMS,
                    PropostaItem.ST AS PropostaItem__ST,
                    PropostaItem.IPI AS PropostaItem__IPI,
                    PropostaItem.PercIPI as PropostaItem__PercIPI,
                    PropostaItem.ValorFundoCombPobreza as PropostaItem__ValorFundoCombPobreza,
                    PropostaItem.Cofins AS PropostaItem__Cofins,
                    PropostaItem.ValorCOFINS as PropostaItem__ValorCOFINS,
                    PropostaItem.MVA as PropostaItem__MVA,
                    PropostaItem.ValorFCPST as PropostaItem__ValorFCPST,
                    PropostaItem.ValorICMSPartilhaOrigem as PropostaItem__ValorICMSPartilhaOrigem,
                    PropostaItem.ValorICMSPartilhaDestino as PropostaItem__ValorICMSPartilhaDestino,
                    PropostaItem.Percentual AS PropostaItem__Percentual,
                    CHARINDEX('ERRO:', PropostaItem.MessageError) AS PropostaItem__Error,
                    PropostaItem.Curva AS PropostaItem__CurvaCliente,
                    PropostaItem.TipoCusto AS PropostaItem__TipoCusto,
                    PropostaItem.Invisivel AS PropostaItem__Invisivel,
                    PropostaItem.Status AS PropostaItem__Status,
                    PropostaItem.FlagCustoAlterado AS PropostaItem__FlagCustoAlterado,
                CASE PropostaItem.Status
                    WHEN 0 THEN 'ITEM DO MIX'
                    WHEN 1 THEN 'FORA DO MIX'
                    WHEN 2 THEN 'COTAR ITEM'
                    WHEN 3 THEN 'MAIS INFORMAÇÕES'
                END AS PropostaItem__NmStatus,
                    Item.ItemID AS PropostaItem__ItemID,
                    Item.SubFamiliaID AS PropostaItem__SubFamiliaID,
                    Item.CdItem AS PropostaItem__CdItem,
                    Item.NmItem AS PropostaItem__NmItem,
                    Item.SegmentoID AS PropostaItem__SegmentoID,
                    Item.FamiliaID AS PropostaItem__FamiliaID,
                    Item.NumCA AS PropostaItem__NumCA,
                    Clas.CdClassificacaoFiscal AS PropostaItem__NCM,
                    Segmento.NmSegmento AS PropostaItem__NmSegmento,
                    Familia.NmFamilia AS PropostaItem__NmFamilia,
                    SubFamilia.NmSubFamilia AS PropostaItem__NmSubFamilia,
                    Item.Curva AS PropostaItem__CurvaBR,
                CASE WHEN ISNULL(Item.FlagOutlet, 0) = 1 THEN 'Y'
                ELSE CASE WHEN ISNULL(Item.FlagSobDemanda, 0) = 1 THEN 'Z'
                ELSE 'X' END
                END AS PropostaItem__Criticidade,
                    Convert(Integer,(ISNULL(EstoqueSIC.QtDispEstoque,0) - ISNULL(EstoqueSIC.QtAlocadaSemOV,0))) AS PropostaItem__QtEstoqueSIC,
                    Preco.Preco_Base AS PropostaItem__PrecoBase,
                    ISNULL(PropostaItem.VlrTabelaPreco, 0) AS PropostaItem__PrecoTabela,
                    ISNULL(PropostaItem.VlrPrecoMinimo, 0) AS PropostaItem__PrecoMinimo,
                    TP.NmTblPreco AS PropostaItem__NomeTabela,
                (SELECT Cod_Barras
                    FROM BrWeb.dbo.Preco_Itens PII
                    WHERE PII.Produto = PropostaItem.CodItemBR
                ) AS PropostaItem__CodBarras
                FROM BrWeb.dbo.Proposta_Itens PropostaItem (NOLOCK)
                INNER JOIN BrWeb..Proposta Proposta (NOLOCK) ON PropostaItem.PropostaID = Proposta.PropostaID
                LEFT JOIN BrSupply.dbo.BR_Estabelecimento Estabelecimento (NOLOCK) ON Proposta.EstabelecimentoID = Estabelecimento.EstabelecimentoID
                LEFT JOIN BrSupply.dbo.BR_Item Item (NOLOCK) ON Item.CdItem = PropostaItem.CodItemBR
                LEFT JOIN BrSupply.dbo.BR_ClassificacaoFiscal Clas on Clas.ClassificacaoFiscalID = Item.ClassificacaoFiscalID
                LEFT JOIN BrSupply.dbo.BR_PrecoEstoque EstoqueSIC (NOLOCK) ON EstoqueSIC.EstabelecimentoID = Proposta.EstabelecimentoID AND EstoqueSIC.ItemID = Item.ItemID
                LEFT JOIN BrWeb..Preco_Itens Preco (NOLOCK) ON Preco.Produto = PropostaItem.CodItemBR
                LEFT JOIN BrSupply.dbo.BR_Segmento Segmento (NOLOCK) ON Item.SegmentoID = Segmento.SegmentoID
                LEFT JOIN BrSupply.dbo.BR_Familia Familia (NOLOCK) ON Item.FamiliaID = Familia.FamiliaID
                LEFT JOIN BrSupply.dbo.BR_SubFamilia SubFamilia (NOLOCK) ON Item.SubFamiliaID = SubFamilia.SubFamiliaID
                LEFT JOIN BrSupply.dbo.BR_TblPreco TP (NOLOCK) ON TP.TblPrecoID = Proposta.TabelaPrecoID
                WHERE PropostaItem.PropostaID = @PropostaID
        """;

    // ── BuscarCatalogo ────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoCatalogoItem>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(BuscarCatalogoSql, connection);
        cmd.CommandTimeout = 120;
        cmd.Parameters.AddWithValue("@Descricao", descricao ?? string.Empty);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);
        cmd.Parameters.AddWithValue("@TblPrecoID", tblPrecoId);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var items = new List<CotacaoCatalogoItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CotacaoCatalogoItem
            {
                ItemID = GetInt32(reader, "ItemID"),
                CdItem = GetString(reader, "CdItem"),
                NmItem = GetString(reader, "NmItem"),
                SegmentoID = GetInt32(reader, "SegmentoID"),
                NmSegmento = GetString(reader, "NmSegmento"),
                FamiliaID = GetInt32(reader, "FamiliaID"),
                NmFamilia = GetString(reader, "NmFamilia"),
                SubFamiliaID = GetInt32(reader, "SubFamiliaID"),
                NmSubFamilia = GetString(reader, "NmSubFamilia"),
                EstabelecimentoID = GetInt32(reader, "EstabelecimentoID"),
                Curva = GetString(reader, "Curva"),
                QtdDisponivel = GetInt32(reader, "QtdDisponivel"),
                QtEstoqueSIC = GetInt32(reader, "QtEstoqueSIC"),
                Ativo = GetString(reader, "Ativo"),
                VlrCustoAquisicao = GetDecimal(reader, "VlrCustoAquisicao"),
                VlrCustoMedio = GetDecimal(reader, "VlrCustoMedio"),
                VlrTabela = GetDecimal(reader, "VlrTabela"),
                VlrPrecoMinimo = GetDecimal(reader, "VlrPrecoMinimo"),
                Criticidade = GetString(reader, "Criticidade"),
                TabelaPreco = GetString(reader, "TabelaPreco"),
            });
        }

        return items;
    }

    // ── GetList ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoListItem>> GetListAsync(
        int? usuarioId,
        int filtroCotacao,
        string? cdExtCliente,
        int? propostaId,
        string? cnpj,
        int? estabelecimentoId,
        int? statusId,
        DateTime dataInicial,
        DateTime dataFinal,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(BaseListSql, connection);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId.HasValue ? usuarioId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@FiltroCotacao", filtroCotacao);
        cmd.Parameters.AddWithValue("@CdExtCliente", string.IsNullOrWhiteSpace(cdExtCliente) ? DBNull.Value : cdExtCliente);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId.HasValue ? propostaId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@CNPJ", string.IsNullOrWhiteSpace(cnpj) ? DBNull.Value : cnpj);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId.HasValue ? estabelecimentoId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@StatusID", statusId.HasValue ? statusId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@DataInicial", dataInicial.Date);
        cmd.Parameters.AddWithValue("@DataFinal", dataFinal.Date.AddDays(1).AddSeconds(-1));

        var items = new List<CotacaoListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CotacaoListItem
            {
                CdExtCliente        = ReadString(reader, "Proposta__CdExtCliente"),
                PropostaId          = ReadInt(reader, "Proposta__PropostaId"),
                CdProposta          = ReadString(reader, "Proposta__CdProposta"),
                Nome                = ReadString(reader, "Proposta__Nome"),
                DtCriacao           = ReadString(reader, "Proposta__DtCriacao"),
                ClienteId           = ReadInt(reader, "Proposta__ClienteId"),
                ClienteNome         = ReadString(reader, "Cliente__Nome"),
                ClienteCNPJ         = ReadString(reader, "Cliente__CNPJ"),
                MargemPadrao        = ReadDecimal(reader, "Proposta__MargemPadrao"),
                Frete               = ReadDecimal(reader, "Proposta__Frete"),
                DataValidade        = ReadString(reader, "Proposta__DataValidade"),
                DataValidadeSQL     = ReadString(reader, "Proposta__DataValidadeSQL"),
                StatusID            = ReadInt(reader, "Proposta__StatusID"),
                StatusName          = ReadString(reader, "Proposta__StatusName"),
                Obs                 = ReadString(reader, "Proposta__Obs"),
                NmMotivo            = ReadString(reader, "Proposta__NmMotivo"),
                Justificativa       = ReadString(reader, "Proposta__Justificativa"),
                CotacaoID           = ReadNullableInt(reader, "Proposta__CotacaoID"),
                CotacaoStatusID     = ReadNullableInt(reader, "Cotacao__StatusID"),
                CotacaoStatus       = ReadString(reader, "Cotacao__Status"),
                TotalVenda          = ReadDecimal(reader, "Proposta__TotalVenda"),
                TipoCotacao         = ReadString(reader, "Proposta__TipoCotacao"),
                NmCondPagto         = ReadString(reader, "Proposta__NmCondPagto"),
                Endereco            = ReadString(reader, "Proposta__Endereco"),
                QtdItens            = ReadInt(reader, "Proposta__QtdItens"),
                EstabelecimentoID   = ReadNullableInt(reader, "Proposta__EstabelecimentoID"),
                NmEstabelecimento   = ReadString(reader, "Proposta__NmEstabelecimento"),
                DataAbertura        = ReadString(reader, "Proposta__DataAbertura"),
                DataAberturaSQL     = ReadString(reader, "Proposta__DataAberturaSQL"),
                Executivo           = ReadString(reader, "Proposta__Executivo"),
                AprovadorNmUsuario  = ReadString(reader, "Aprovador__NmUsuario"),
            });
        }

        return items;
    }

    // ── GetByPropostaId ───────────────────────────────────────────────────────

    public async Task<CotacaoDetalhe?> GetByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(DetalheSql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var detalhe = MapRowToDetalhe(reader);
        await reader.CloseAsync();

        const string creditoSql = "SELECT BrSupply.dbo.fn_BR_ValidaCredito(@ClienteID, 0) AS StatusCredito";
        await using var cmdCredito = new SqlCommand(creditoSql, connection);
        cmdCredito.Parameters.AddWithValue("@ClienteID", detalhe.ClienteID);
        var statusCredito = await cmdCredito.ExecuteScalarAsync(cancellationToken);
        detalhe.StatusCredito = statusCredito?.ToString() ?? string.Empty;

        if (detalhe.ConsultorUsuarioID > 0)
        {
            const string atendenteSql = """
                SELECT ISNULL(U.FlagPrecisaAprovacao, 0) AS FlagPrecisaAprovacao,
                       ISNULL(U.PercMargemMinPedido, 0)  AS PercMargemMinPedido,
                       ISNULL(U.PercMargemMaxPedido, 0)  AS PercMargemMaxPedido,
                       U.AprovadorID                     AS AprovadorID,
                       ISNULL(Aprov.NmUsuario, '')       AS AprovadorNmUsuario
                FROM BrSupply.dbo.BR_Usuario U (NOLOCK)
                LEFT JOIN BrSupply.dbo.BR_Usuario Aprov (NOLOCK) ON Aprov.UsuarioID = U.AprovadorID
                WHERE U.UsuarioID = @AtendenteID
                """;
            await using var cmdAtendente = new SqlCommand(atendenteSql, connection);
            cmdAtendente.Parameters.AddWithValue("@AtendenteID", detalhe.ConsultorUsuarioID.Value);
            await using var readerAtendente = await cmdAtendente.ExecuteReaderAsync(cancellationToken);
            if (await readerAtendente.ReadAsync(cancellationToken))
            {
                detalhe.FlagPrecisaAprovacao   = ReadInt(readerAtendente, "FlagPrecisaAprovacao") == 1;
                detalhe.PercMargemMinPedido    = ReadDecimal(readerAtendente, "PercMargemMinPedido");
                detalhe.PercMargemMaxPedido    = ReadDecimal(readerAtendente, "PercMargemMaxPedido");
                detalhe.AtendenteAprovadorID   = ReadInt(readerAtendente, "AprovadorID");
                detalhe.AtendenteAprovadorNome = ReadString(readerAtendente, "AprovadorNmUsuario");
            }
        }

        detalhe.Itens = await GetItensByPropostaIdAsync(propostaId, cancellationToken);
        return detalhe;
    }

    // ── GetItensByPropostaId ──────────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoDetalheItem>> GetItensByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(PropostaItensSql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var items = new List<CotacaoDetalheItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(MapRowToDetalheItem(reader));

        return items;
    }

    // ── GetEstabelecimentoOptions ─────────────────────────────────────────────

    public Task<IReadOnlyList<CotacaoSelectOption>> GetEstabelecimentoOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT
                Proposta.EstabelecimentoID AS Id,
                BR_Estabelecimento.NmEstabelecimento AS Nome
            FROM BrWeb..Proposta Proposta WITH (NOLOCK)
            LEFT JOIN BrSupply..BR_Estabelecimento BR_Estabelecimento WITH (NOLOCK)
                ON BR_Estabelecimento.EstabelecimentoID = Proposta.EstabelecimentoID
            WHERE Proposta.TipoID = 2
              AND Proposta.EstabelecimentoID IS NOT NULL
              AND BR_Estabelecimento.NmEstabelecimento IS NOT NULL
            ORDER BY Nome
            """;
        return GetSelectOptionsAsync(sql, cancellationToken);
    }

    // ── GetStatusOptions ──────────────────────────────────────────────────────

    public Task<IReadOnlyList<CotacaoSelectOption>> GetStatusOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT
                Proposta.StatusID AS Id,
                Status.NmStatus AS Nome
            FROM BrWeb..Proposta Proposta WITH (NOLOCK)
            INNER JOIN BrWeb..Proposta_Status Status WITH (NOLOCK)
                ON Proposta.StatusID = Status.StatusID
            WHERE Proposta.TipoID = 2
            ORDER BY Nome
            """;
        return GetSelectOptionsAsync(sql, cancellationToken);
    }

    // ── GetCondicoesPagamento ─────────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoSelectOption>> GetCondicoesPagamentoAsync(
        int estabelecimentoId,
        decimal valorTotal,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CP.CondPagtoID AS Id,
                   CP.DsCondPagto AS Nome
            FROM BRsupply.dbo.BR_CondPagto CP (NOLOCK)
            INNER JOIN BrWeb.dbo.TelevendasConfigCondPagto Tel (NOLOCK)
                ON Tel.CondPagtoID = CP.CondPagtoID
               AND CP.FlagAtivo = 1
               AND CP.FlagPagarReceber = 'R'
               AND Tel.EstabelecimentoID = @EstabelecimentoID
            WHERE @ValorTotal >= Tel.VlrMinimo
            ORDER BY CP.DsCondPagto
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);
        cmd.Parameters.AddWithValue("@ValorTotal", valorTotal);

        var items = new List<CotacaoSelectOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoSelectOption { Id = ReadInt(reader, "Id"), Nome = ReadString(reader, "Nome") });

        return items;
    }

    // ── GetExecutivoVendas ────────────────────────────────────────────────────

    public async Task<string> GetExecutivoVendasAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 U.NmUsuario
            FROM BrSupply.dbo.BR_Cliente C (NOLOCK)
            INNER JOIN BrSupply.dbo.BR_Carteira K (NOLOCK) ON K.CarteiraID = C.CarteiraID
            INNER JOIN BrSupply.dbo.BR_Usuario U (NOLOCK) ON U.UsuarioID = K.ExecVendasID
            WHERE C.ClienteID = @ClienteID
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result?.ToString() ?? string.Empty;
    }

    // ── CalcularFreteProposta ─────────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoFreteOpcao>> CalcularFretePropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM BrSupply.dbo.Fn_Calcula_Fretes_Proposta(@PropostaID)";
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var list = new List<CotacaoFreteOpcao>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new CotacaoFreteOpcao
            {
                TransportadoraID          = ReadInt(reader, "TransportadoraID"),
                Nome                      = ReadString(reader, "Nome"),
                TempoLogistico            = ReadInt(reader, "TempoLogistico"),
                TempoComercial            = ReadInt(reader, "TempoComercial"),
                TaxaExtra                 = ReadDecimal(reader, "TaxaExtra"),
                ValorFrete                = ReadDecimal(reader, "ValorFrete"),
                QtItensRestritos          = ReadInt(reader, "QtItensRestritos"),
                FlagObrigatoriaCanalVenda = ReadBool(reader, "FlagObrigatoriaCanalVenda"),
                FlagClienteRestrito       = ReadBool(reader, "FlagClienteRestrito"),
                FlagClienteFixo           = ReadBool(reader, "FlagClienteFixo"),
            });
        }

        return list;
    }

    // ── GetImpostosItem ───────────────────────────────────────────────────────

    public async Task<CotacaoItemImpostos?> GetImpostosItemAsync(
        int propostaItemId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CodItemBR,
                   CONVERT(VARCHAR(20), VlrPrecoMargem) + '%'         AS MB,
                   FORMAT(ValorLiqUnit,  'C', 'pt-BR')                AS VlrLiqUnit,
                   CONVERT(VARCHAR(20), ICM) + '%'                    AS PercICMS,
                   FORMAT(ValorICMS,    'C', 'pt-BR')                 AS VlrICMS,
                   CONVERT(VARCHAR(20), PercIPI) + '%'                AS PercIPI,
                   FORMAT(IPI,          'C', 'pt-BR')                 AS VlrIPI,
                   FORMAT(ValorFundoCombPobreza, 'C', 'pt-BR')        AS VlrFCP,
                   CONVERT(VARCHAR(20), Pis) + '%'                    AS PercPIS,
                   FORMAT(ValorPIS,     'C', 'pt-BR')                 AS VlrPIS,
                   CONVERT(VARCHAR(20), Cofins) + '%'                 AS PercCOFINS,
                   FORMAT(ValorCOFINS,  'C', 'pt-BR')                 AS VlrCOFINS,
                   FORMAT(MVA,          'C', 'pt-BR')                 AS MVA,
                   FORMAT(ST,           'C', 'pt-BR')                 AS ST,
                   FORMAT(ValorFCPST,   'C', 'pt-BR')                 AS VlrFCPST,
                   FORMAT(ValorICMSPartilhaOrigem,  'C', 'pt-BR')     AS VlrICMSPartOrigem,
                   FORMAT(ValorICMSPartilhaDestino, 'C', 'pt-BR')     AS VlrICMSPartDestino
            FROM BrWeb.dbo.Proposta_Itens
            WHERE PropostaItemID = @PropostaItemID
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PropostaItemID", propostaItemId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return new CotacaoItemImpostos
        {
            CodItemBR          = ReadString(reader, "CodItemBR"),
            MB                 = ReadString(reader, "MB"),
            VlrLiqUnit         = ReadString(reader, "VlrLiqUnit"),
            PercICMS           = ReadString(reader, "PercICMS"),
            VlrICMS            = ReadString(reader, "VlrICMS"),
            PercIPI            = ReadString(reader, "PercIPI"),
            VlrIPI             = ReadString(reader, "VlrIPI"),
            VlrFCP             = ReadString(reader, "VlrFCP"),
            PercPIS            = ReadString(reader, "PercPIS"),
            VlrPIS             = ReadString(reader, "VlrPIS"),
            PercCOFINS         = ReadString(reader, "PercCOFINS"),
            VlrCOFINS          = ReadString(reader, "VlrCOFINS"),
            MVA                = ReadString(reader, "MVA"),
            ST                 = ReadString(reader, "ST"),
            VlrFCPST           = ReadString(reader, "VlrFCPST"),
            VlrICMSPartOrigem  = ReadString(reader, "VlrICMSPartOrigem"),
            VlrICMSPartDestino = ReadString(reader, "VlrICMSPartDestino"),
        };
    }

    // ── ValidarItensImportacao ────────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoItemValidacao>> ValidarItensImportacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SET NOCOUNT ON
            EXEC BrWeb.dbo.BR_SP_ValidaItensProposta @PropostaID = @PropostaID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.CommandTimeout = 60;
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var items = new List<CotacaoItemValidacao>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CotacaoItemValidacao
            {
                CdItem            = ReadString(reader, "CdItem"),
                NmItem            = ReadString(reader, "NmItem"),
                VlrUnit           = ReadDecimal(reader, "VlrUnit"),
                VlrPrecoMinimo    = TryReadDecimal(reader, "VlrPrecoMinimo"),
                VlrCustoAquisicao = ReadDecimal(reader, "VlrCustoAquisicao"),
                VlrCustoMedio     = ReadDecimal(reader, "VlrCustoMedio"),
            });
        }

        return items;
    }

    // ── GetEnviarEmailDados ───────────────────────────────────────────────────

    public async Task<CotacaoDadosEmail?> GetEnviarEmailDadosAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Proposta.PropostaId                                             AS PropostaId,
                ISNULL(Proposta.CotacaoID, 0)                                   AS CotacaoID,
                Proposta.EstabelecimentoID                                      AS EstabelecimentoID,
                Proposta.ClienteId                                              AS ClienteId,
                Proposta.CdProposta                                             AS CdProposta,
                Estabelecimento.NmEstabelecimento                               AS EstabelecimentoNome,
                (ISNULL(Cliente.CdExtCliente, '') + ' - ' + ISNULL(Cliente.NmCliente, '')) AS ClienteNome,
                (ISNULL(ClienteEndereco.Cidade, '') + ' - ' + ISNULL(UF.CdUF, '')) AS ClienteCidadeEstado,
                Proposta.ContatoNome                                            AS ContatoNome,
                Proposta.ContatoEmail                                           AS ContatoEmail,
                Consultor.NmUsuario                                             AS ConsultorNome,
                Consultor.Email                                                 AS ConsultorEmail,
                Executivo.NmUsuario                                             AS ExecutivoNome,
                Executivo.Email                                                 AS ExecutivoEmail,
                FORMAT((SELECT SUM(PItem.VlrPrecoVenda) FROM BrWeb..Proposta_Itens AS PItem WITH (NOLOCK) WHERE PItem.PropostaID = Proposta.PropostaId), 'C', 'pt-BR') AS TotalVenda,
                FORMAT(ISNULL(Proposta.Frete, 0), 'C', 'pt-BR')                AS Frete
            FROM BrWeb.dbo.Proposta Proposta (NOLOCK)
                LEFT JOIN BrSupply.dbo.BR_Estabelecimento Estabelecimento (NOLOCK) ON Estabelecimento.EstabelecimentoID = Proposta.EstabelecimentoID
                LEFT JOIN BrSupply.dbo.BR_Usuario Consultor (NOLOCK) ON Consultor.UsuarioID = Proposta.UsuarioID
                LEFT JOIN BrSupply.dbo.BR_Cliente Cliente (NOLOCK) ON Cliente.ClienteID = Proposta.ClienteId
                    LEFT JOIN BrSupply.dbo.BR_Carteira Carteira (NOLOCK) ON Carteira.CarteiraID = Cliente.CarteiraID
                        LEFT JOIN BrSupply.dbo.BR_Usuario Executivo (NOLOCK) ON Executivo.UsuarioID = Carteira.ExecVendasID
                LEFT JOIN BrSupply.dbo.BR_ClienteEndereco ClienteEndereco (NOLOCK) ON ClienteEndereco.ClienteEnderecoID = Proposta.ClienteEnderecoID
                    LEFT JOIN BrSupply.dbo.BR_UF UF (NOLOCK) ON UF.UFID = ClienteEndereco.UFID
            WHERE Proposta.PropostaId = @PropostaID
            ORDER BY Proposta.PropostaId DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return new CotacaoDadosEmail
        {
            PropostaId          = ReadInt(reader, "PropostaId"),
            CotacaoID           = ReadInt(reader, "CotacaoID"),
            EstabelecimentoID   = ReadInt(reader, "EstabelecimentoID"),
            ClienteId           = ReadInt(reader, "ClienteId"),
            CdProposta          = ReadString(reader, "CdProposta"),
            EstabelecimentoNome = ReadString(reader, "EstabelecimentoNome"),
            ClienteNome         = ReadString(reader, "ClienteNome"),
            ClienteCidadeEstado = ReadString(reader, "ClienteCidadeEstado"),
            ContatoNome         = ReadString(reader, "ContatoNome"),
            ContatoEmail        = ReadString(reader, "ContatoEmail"),
            ConsultorNome       = ReadString(reader, "ConsultorNome"),
            ConsultorEmail      = ReadString(reader, "ConsultorEmail"),
            ExecutivoNome       = ReadString(reader, "ExecutivoNome"),
            ExecutivoEmail      = ReadString(reader, "ExecutivoEmail"),
            TotalVenda          = ReadString(reader, "TotalVenda"),
            Frete               = ReadString(reader, "Frete"),
        };
    }

    // ── GetHistoricoEnvios ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoEnvioHistoricoItem>> GetHistoricoEnviosAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                E.PropostaCotacaoEnvioID,
                E.Nome,
                E.Email,
                CONVERT(VARCHAR(10), E.DataHora, 103) + ' ' + CONVERT(VARCHAR(5), E.DataHora, 108) AS DtEnvio,
                U.NmUsuario,
                CONVERT(VARCHAR(10), E.DataHoraVisualizacao, 103) + ' ' + CONVERT(VARCHAR(5), E.DataHoraVisualizacao, 108) AS DtVisualizacao,
                CASE FlagVisualizaEstoque WHEN 0 THEN 'N' ELSE 'S' END AS FlagVisualizaEstoque,
                CASE FlagPodeNegociar WHEN 0 THEN 'N' ELSE 'S' END AS FlagPodeNegociar,
                CASE FlagPodetrocartransportadora WHEN 0 THEN 'N' ELSE 'S' END AS FlagPodeTrocarTransportadora,
                CASE FlagPodeTrocarCondPagto WHEN 0 THEN 'N' ELSE 'S' END AS FlagPodeTrocarCondPagto,
                E.FlagAtivo
            FROM BRWeb..Proposta_CotacaoEnvio E,
                 BrSupply..BR_Usuario U
            WHERE E.UsuarioID = U.UsuarioID
              AND E.PropostaID = @PropostaID
            ORDER BY E.DataHora DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var items = new List<CotacaoEnvioHistoricoItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CotacaoEnvioHistoricoItem
            {
                PropostaCotacaoEnvioID       = ReadInt(reader, "PropostaCotacaoEnvioID"),
                Nome                         = ReadString(reader, "Nome"),
                Email                        = ReadString(reader, "Email"),
                DtEnvio                      = ReadString(reader, "DtEnvio"),
                NmUsuario                    = ReadString(reader, "NmUsuario"),
                DtVisualizacao               = ReadString(reader, "DtVisualizacao"),
                FlagVisualizaEstoque         = ReadString(reader, "FlagVisualizaEstoque"),
                FlagPodeNegociar             = ReadString(reader, "FlagPodeNegociar"),
                FlagPodeTrocarTransportadora = ReadString(reader, "FlagPodeTrocarTransportadora"),
                FlagPodeTrocarCondPagto      = ReadString(reader, "FlagPodeTrocarCondPagto"),
                FlagAtivo                    = ReadInt(reader, "FlagAtivo"),
            });
        }

        return items;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<IReadOnlyList<CotacaoSelectOption>> GetSelectOptionsAsync(
        string sql,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        var items = new List<CotacaoSelectOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var idOrdinal   = reader.GetOrdinal("Id");
            var nomeOrdinal = reader.GetOrdinal("Nome");
            items.Add(new CotacaoSelectOption
            {
                Id   = reader.IsDBNull(idOrdinal) ? 0
                     : reader.GetFieldType(idOrdinal) == typeof(int)
                         ? reader.GetInt32(idOrdinal)
                         : Convert.ToInt32(reader.GetValue(idOrdinal)),
                Nome = reader.IsDBNull(nomeOrdinal) ? string.Empty : reader.GetString(nomeOrdinal),
            });
        }

        return items;
    }

    private static CotacaoDetalhe MapRowToDetalhe(SqlDataReader reader) => new()
    {
        PropostaID                    = ReadInt(reader, "PropostaID"),
        CdProposta                    = ReadString(reader, "CdProposta"),
        Nome                          = ReadString(reader, "Nome"),
        Versao                        = ReadInt(reader, "Versao"),
        OrdemCompra                   = ReadString(reader, "OrdemCompra"),
        StatusID                      = ReadInt(reader, "StatusID"),
        StatusNome                    = ReadString(reader, "StatusNome"),
        TipoCotacao                   = ReadString(reader, "TipoCotacao"),
        DataValidade                  = ReadString(reader, "DataValidade"),
        EstabelecimentoID             = ReadInt(reader, "EstabelecimentoID"),
        EstabelecimentoNome           = ReadString(reader, "EstabelecimentoNome"),
        EstabelecimentoCNPJ           = ReadString(reader, "EstabelecimentoCNPJ"),
        EstabelecimentoRazaoSocial    = ReadString(reader, "EstabelecimentoRazaoSocial"),
        ClienteID                     = ReadInt(reader, "ClienteID"),
        ClienteCodigo                 = ReadString(reader, "ClienteCodigo"),
        ClienteNome                   = ReadString(reader, "ClienteNome"),
        ClienteCodNome                = ReadString(reader, "ClienteCodNome"),
        ClienteCNPJ                   = ReadString(reader, "ClienteCNPJ"),
        ClienteContribuinte           = ReadString(reader, "ClienteContribuinte"),
        EhContribuinte                = ReadBool(reader, "EhContribuinte"),
        ClienteEnderecoID             = ReadInt(reader, "ClienteEnderecoID"),
        ClienteEndereco               = ReadString(reader, "ClienteEndereco"),
        ClienteCidadeEstado           = ReadString(reader, "ClienteCidadeEstado"),
        ClienteLocalEntregaID         = ReadInt(reader, "ClienteLocalEntregaID"),
        LocalEntregaNome              = ReadString(reader, "LocalEntregaNome"),
        LocalEntregaEndereco          = ReadString(reader, "LocalEntregaEndereco"),
        LocalEntregaCidadeEstado      = ReadString(reader, "LocalEntregaCidadeEstado"),
        LocalEntregaObservacao        = ReadString(reader, "LocalEntregaObservacao"),
        CanalVenda                    = ReadString(reader, "CanalVenda"),
        TipoOrdem                     = ReadString(reader, "TipoOrdem"),
        TipoOVSAP                     = ReadString(reader, "TipoOVSAP"),
        TipoOVEhRevenda               = ReadBool(reader, "TipoOVEhRevenda"),
        TipoMotivoIDSAP               = ReadNullableInt(reader, "TipoMotivoIDSAP"),
        Motivo                        = ReadString(reader, "Motivo"),
        MotivoNome                    = ReadString(reader, "MotivoNome"),
        Justificativa                 = ReadString(reader, "Justificativa"),
        AprovadorUsuarioID            = ReadNullableInt(reader, "AprovadorUsuarioID"),
        AprovadorNome                 = ReadString(reader, "AprovadorNome"),
        AprovadorJustificativa        = ReadString(reader, "AprovadorJustificativa"),
        CondPagtoID                   = ReadNullableInt(reader, "CondPagtoID"),
        CondPagtoNome                 = ReadString(reader, "CondPagtoNome"),
        FormaPagamentoSAP             = ReadNullableInt(reader, "FormaPagamentoSAP"),
        FormaPagamentoDesc            = ReadString(reader, "FormaPagamentoDesc"),
        FlagDefCondPagTelevendas      = ReadBool(reader, "FlagDefCondPagTelevendas"),
        TabelaPrecoID                 = ReadString(reader, "TabelaPrecoID"),
        TabelaPrecoNome               = ReadString(reader, "TabelaPrecoNome"),
        FlagPrecoConformeTabela       = ReadBool(reader, "FlagPrecoConformeTabela"),
        MargemPadrao                  = ReadDecimal(reader, "MargemPadrao"),
        MargemBruta                   = ReadDecimal(reader, "MargemBruta"),
        MargemContribuida             = ReadDecimal(reader, "MargemContribuida"),
        MargemBrutaFixa               = ReadDecimal(reader, "MargemBrutaFixa"),
        MargemContribuidaFixa         = ReadDecimal(reader, "MargemContribuidaFixa"),
        Frete                         = ReadString(reader, "Frete"),
        ValorVendaTotal               = ReadDecimal(reader, "ValorVendaTotal"),
        VlrContribTotal               = ReadDecimal(reader, "VlrContribTotal"),
        ValorContribuicaoFixo         = ReadDecimal(reader, "ValorContribuicaoFixo"),
        ValorTotalFixo                = ReadDecimal(reader, "ValorTotalFixo"),
        VlrPedidoMinimo               = ReadDecimal(reader, "VlrPedidoMinimo"),
        TotalVenda                    = ReadString(reader, "TotalVenda"),
        TotalVendaFrete               = ReadString(reader, "TotalVendaFrete"),
        TotalVendaSemImposto          = ReadString(reader, "TotalVendaSemImposto"),
        TotalVendaFreteSemImposto     = ReadString(reader, "TotalVendaFreteSemImposto"),
        TotalPeso                     = ReadDecimal(reader, "TotalPeso"),
        QtdItens                      = ReadInt(reader, "QtdItens"),
        DiasPrazoEntrega              = ReadInt(reader, "DiasPrazoEntrega"),
        DataProgEntrega               = ReadString(reader, "DataProgEntrega"),
        NatOperacao                   = ReadString(reader, "NatOperacao"),
        UfOrigem                      = ReadString(reader, "UfOrigem"),
        UfDestino                     = ReadString(reader, "UfDestino"),
        CodigoIBGE                    = ReadString(reader, "CodigoIBGE"),
        ContatoNome                   = ReadString(reader, "ContatoNome"),
        ContatoEmail                  = ReadString(reader, "ContatoEmail"),
        TransportadoraID              = ReadNullableInt(reader, "TransportadoraID"),
        TransportadoraNome            = ReadString(reader, "TransportadoraNome"),
        CotacaoID                     = ReadNullableInt(reader, "CotacaoID"),
        CotacaoIdOriginal             = ReadNullableInt(reader, "CotacaoIdOriginal"),
        CotacaoStatusDesc             = ReadString(reader, "CotacaoStatusDesc"),
        CotacaoEnvioComentarios       = ReadString(reader, "CotacaoEnvioComentarios"),
        FlagRevisarValorProdutos      = ReadBool(reader, "FlagRevisarValorProdutos"),
        FlagRevisarValorFrete         = ReadBool(reader, "FlagRevisarValorFrete"),
        FlagRevisarPrazoPagamento     = ReadBool(reader, "FlagRevisarPrazoPagamento"),
        FlagRevisarPrazoEntrega       = ReadBool(reader, "FlagRevisarPrazoEntrega"),
        FlagRevisarAtendimento        = ReadBool(reader, "FlagRevisarAtendimento"),
        FlagRevisarPermiteTrocarMarca = ReadBool(reader, "FlagRevisarPermiteTrocarMarca"),
        FlagRevisarPermiteTrocarUnidade = ReadBool(reader, "FlagRevisarPermiteTrocarUnidade"),
        FlagPrecosInformados          = ReadBool(reader, "FlagPrecosInformados"),
        CotacaoEnvioIPAprovacao       = ReadString(reader, "CotacaoEnvioIPAprovacao"),
        ConsultorUsuarioID            = ReadNullableInt(reader, "ConsultorUsuarioID"),
        ConsultorNome                 = ReadString(reader, "ConsultorNome"),
        ConsultorEmail                = ReadString(reader, "ConsultorEmail"),
        CarteiraNome                  = ReadString(reader, "CarteiraNome"),
        Observacao                    = ReadString(reader, "Observacao"),
        Obs                           = ReadString(reader, "Obs"),
    };

    private static CotacaoDetalheItem MapRowToDetalheItem(SqlDataReader reader)
    {
        var quantidade        = ReadDecimal(reader, "PropostaItem__Quantidade");
        var precoItem         = ReadDecimal(reader, "PropostaItem__PrecoItem");
        var vlrPrecoVenda     = ReadDecimal(reader, "PropostaItem__VlrPrecoVenda");
        var valorIcms         = ReadDecimal(reader, "PropostaItem__ValorICMS");
        var ipi               = ReadDecimal(reader, "PropostaItem__IPI");
        var st                = ReadDecimal(reader, "PropostaItem__ST");
        var tipoCusto         = ReadString(reader, "PropostaItem__TipoCusto");
        var vlrCustoAquisicao = ReadDecimal(reader, "PropostaItem__VlrCustoAquisicao");
        var vlrCustoMedio     = ReadDecimal(reader, "PropostaItem__VlrCustoMedio");

        return new CotacaoDetalheItem
        {
            PropostaItemID             = ReadInt(reader, "PropostaItem__PropostaItemID"),
            PropostaID                 = ReadInt(reader, "PropostaItem__PropostaID"),
            ProdutoID                  = ReadNullableInt(reader, "PropostaItem__ItemID"),
            CodigoProduto              = ReadString(reader, "PropostaItem__CodItemBR"),
            DescricaoProduto           = ReadString(reader, "PropostaItem__DescrItemBR"),
            UnidadeMedida              = ReadString(reader, "PropostaItem__UniMedBr"),
            Quantidade                 = quantidade,
            EstoqueDisponivel          = ReadDecimal(reader, "PropostaItem__QtEstoqueSIC"),
            PrecoMinimo                = ReadDecimal(reader, "PropostaItem__PrecoMinimo"),
            PrecoTabelaPreco           = ReadDecimal(reader, "PropostaItem__PrecoTabela"),
            TipoCusto                  = string.IsNullOrWhiteSpace(tipoCusto) ? "A" : tipoCusto,
            VlrCustoAquisicao          = vlrCustoAquisicao,
            VlrCustoMedio              = vlrCustoMedio,
            CustoLiquido               = tipoCusto == "M" ? vlrCustoMedio : vlrCustoAquisicao,
            PrecoItem                  = precoItem,
            VlrPrecoVenda              = vlrPrecoVenda,
            Margem                     = ReadDecimal(reader, "PropostaItem__MargemCalculada"),
            MargemPercentual           = ReadDecimal(reader, "PropostaItem__Margem"),
            ICMS                       = ReadDecimal(reader, "PropostaItem__ICM"),
            IPI                        = ipi,
            ST                         = st,
            PIS                        = ReadDecimal(reader, "PropostaItem__Pis"),
            COFINS                     = ReadDecimal(reader, "PropostaItem__Cofins"),
            TotalImpostos              = valorIcms + ipi + st,
            TotalSemImposto            = precoItem * quantidade,
            TotalComImposto            = vlrPrecoVenda,
            ValorLiqUnit               = ReadDecimal(reader, "PropostaItem__ValorLiqUnit"),
            ValorICMS                  = valorIcms,
            PercIPI                    = ReadDecimal(reader, "PropostaItem__PercIPI"),
            ValorFundoCombPobreza      = ReadDecimal(reader, "PropostaItem__ValorFundoCombPobreza"),
            ValorPis                   = ReadDecimal(reader, "PropostaItem__ValorPis"),
            ValorCOFINS                = ReadDecimal(reader, "PropostaItem__ValorCOFINS"),
            ValorFCPST                 = ReadDecimal(reader, "PropostaItem__ValorFCPST"),
            ValorICMSPartilhaOrigem    = ReadDecimal(reader, "PropostaItem__ValorICMSPartilhaOrigem"),
            ValorICMSPartilhaDestino   = ReadDecimal(reader, "PropostaItem__ValorICMSPartilhaDestino"),
            MVA                        = ReadDecimal(reader, "PropostaItem__MVA"),
            NCM                        = ReadString(reader, "PropostaItem__NCM"),
            NumCA                      = ReadString(reader, "PropostaItem__NumCA"),
            SegmentoID                 = ReadInt(reader, "PropostaItem__SegmentoID"),
            NmSegmento                 = ReadString(reader, "PropostaItem__NmSegmento"),
            NmFamilia                  = ReadString(reader, "PropostaItem__NmFamilia"),
            NmSubFamilia               = ReadString(reader, "PropostaItem__NmSubFamilia"),
            CodBarras                  = ReadString(reader, "PropostaItem__CodBarras"),
            NumeroLinha                = ReadNullableInt(reader, "PropostaItem__Numero"),
            Status                     = ReadInt(reader, "PropostaItem__Status"),
            NmStatus                   = ReadString(reader, "PropostaItem__NmStatus"),
            Invisivel                  = ReadBool(reader, "PropostaItem__Invisivel"),
            FlagCustoAlterado          = ReadBool(reader, "PropostaItem__FlagCustoAlterado"),
            Curva                      = ReadString(reader, "PropostaItem__CurvaCliente"),
            Criticidade                = ReadString(reader, "PropostaItem__Criticidade"),
            PrecoBase                  = ReadDecimal(reader, "PropostaItem__PrecoBase"),
            NomeTabela                 = ReadString(reader, "PropostaItem__NomeTabela"),
        };
    }

    private static string GetString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static int GetInt32(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    private static decimal GetDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return string.Empty;
        var fieldType = reader.GetFieldType(ordinal);
        return fieldType == typeof(string)
            ? reader.GetString(ordinal)
            : reader.GetValue(ordinal)?.ToString() ?? string.Empty;
    }

    private static int ReadInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return 0;
        return reader.GetFieldType(ordinal) == typeof(int)
            ? reader.GetInt32(ordinal)
            : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static int? ReadNullableInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return null;
        return reader.GetFieldType(ordinal) == typeof(int)
            ? reader.GetInt32(ordinal)
            : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static decimal ReadDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return 0m;
        return reader.GetFieldType(ordinal) == typeof(decimal)
            ? reader.GetDecimal(ordinal)
            : Convert.ToDecimal(reader.GetValue(ordinal));
    }

    private static decimal TryReadDecimal(SqlDataReader reader, string column)
    {
        try
        {
            var ordinal = reader.GetOrdinal(column);
            if (reader.IsDBNull(ordinal)) return 0m;
            return reader.GetFieldType(ordinal) == typeof(decimal)
                ? reader.GetDecimal(ordinal)
                : Convert.ToDecimal(reader.GetValue(ordinal));
        }
        catch { return 0m; }
    }

    private static bool ReadBool(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return false;
        var fieldType = reader.GetFieldType(ordinal);
        if (fieldType == typeof(bool)) return reader.GetBoolean(ordinal);
        if (fieldType == typeof(int))  return reader.GetInt32(ordinal) != 0;
        if (fieldType == typeof(byte)) return reader.GetByte(ordinal) != 0;
        if (fieldType == typeof(string))
        {
            return reader.GetString(ordinal).Trim().ToUpperInvariant() switch
            {
                "SIM" or "S" or "TRUE" or "T" or "1" or "Y" or "YES" => true,
                _ => false
            };
        }
        try { return Convert.ToBoolean(reader.GetValue(ordinal)); }
        catch { return false; }
    }

    // ─── CotacaoAddService reads ─────────────────────────────────────────────

    public async Task<IReadOnlyList<CotacaoTipoOption>> GetTiposCotacaoAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT t.ID AS CotacaoTipoID, t.NmTipo AS DsCotacaoTipo
            FROM BrWeb..Cotacao_Tipo t WITH (NOLOCK)
            INNER JOIN BrWeb..Intranet_PermissoesUsuario pu WITH (NOLOCK)
                ON pu.PermissaoID = t.PermissaoID
            WHERE t.FlagAtivo = 1
              AND pu.UsuarioID = @UsuarioID
            ORDER BY t.NmTipo
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

        var items = new List<CotacaoTipoOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoTipoOption
            {
                CotacaoTipoId = reader.GetInt32(reader.GetOrdinal("CotacaoTipoID")),
                DsCotacaoTipo = ReadString(reader, "DsCotacaoTipo")
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoSelectOption>> GetMotivosBonificacaoAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT M.Id, M.Descricao
            FROM Integracao_Clientes.dbo.BR_SAP_RelacaoUsuariosMotivos RM WITH (NOLOCK)
            JOIN Integracao_Clientes.dbo.BR_SAP_MotivosPedidos M WITH (NOLOCK)
                ON M.Id = RM.TipoID
            WHERE RM.UsuarioID = @UsuarioID
            ORDER BY M.Descricao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

        var items = new List<CotacaoSelectOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoSelectOption
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nome = ReadString(reader, "Descricao")
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoSelectOption>> GetFormasPagamentoAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, CodFormaPagto, Descricao
            FROM Integracao_Clientes..BR_SAP_FormasPagamento WITH (NOLOCK)
            ORDER BY Descricao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);

        var items = new List<CotacaoSelectOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoSelectOption
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nome = $"{ReadString(reader, "CodFormaPagto")} - {ReadString(reader, "Descricao")}"
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoEstabelecimentoOption>> GetEstabelecimentosAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT E.EstabelecimentoID, E.NmEstabelecimento, UFID
            FROM BrSupply..BR_Estabelecimento E WITH (NOLOCK)
            WHERE E.FlagAtivo = 1
              AND E.EstabelecimentoID NOT IN (7,11,12,13,14)
            ORDER BY E.OrdemExibicao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);

        var items = new List<CotacaoEstabelecimentoOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoEstabelecimentoOption
            {
                EstabelecimentoId = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                Nome = ReadString(reader, "NmEstabelecimento"),
                UfId = reader.IsDBNull(reader.GetOrdinal("UFID")) ? 0 : reader.GetInt32(reader.GetOrdinal("UFID"))
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoUfOption>> GetUfsAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT UFID, CdUF FROM BrSupply.dbo.BR_UF";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);

        var items = new List<CotacaoUfOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoUfOption
            {
                UfId = reader.GetInt32(reader.GetOrdinal("UFID")),
                CdUf = ReadString(reader, "CdUF")
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoClienteSearchResult>> SearchClientesAsync(
        string termo, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        const string sql = "SET NOCOUNT ON EXEC BrSupply.dbo.SIC_PesquisaCliente @Termo, @EstabelecimentoID";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Termo", termo);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);

        var items = new List<CotacaoClienteSearchResult>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoClienteSearchResult
            {
                Id = reader.GetInt32(reader.GetOrdinal("ClienteID")),
                Text = ReadString(reader, "Cliente")
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoEnderecoOption>> GetEnderecosByClienteAsync(
        int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                ClienteEndereco.ClienteEnderecoID,
                ClienteEndereco.Bairro,
                ClienteEndereco.Logradouro,
                ClienteEndereco.Numero,
                ClienteEndereco.CdEMS,
                BrWeb.dbo.FormataCPFCNPJ(ClienteEndereco.CPFCNPJ) AS CNPJ
            FROM BrSupply.dbo.BR_ClienteEndereco ClienteEndereco (NOLOCK)
            WHERE ClienteEndereco.ClienteID = @ClienteID
            ORDER BY ClienteEndereco.Logradouro
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var items = new List<CotacaoEnderecoOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var bairro    = ReadString(reader, "Bairro");
            var logradouro = ReadString(reader, "Logradouro");
            var numero    = ReadString(reader, "Numero");
            var cdEms     = ReadString(reader, "CdEMS");
            items.Add(new CotacaoEnderecoOption
            {
                ClienteEnderecoId = reader.GetInt32(reader.GetOrdinal("ClienteEnderecoID")),
                Text = $"{ReadString(reader, "CNPJ")} - {bairro} | {logradouro} | Nº {numero} ({cdEms})"
            });
        }
        return items;
    }

    public async Task<IReadOnlyList<CotacaoLocalEntregaOption>> GetLocaisEntregaByEnderecoAsync(
        int clienteEnderecoId, CancellationToken cancellationToken = default)
    {
        const string sql = """
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

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await ReadLocaisEntregaInternalAsync(connection, sql, clienteEnderecoId, cancellationToken);
    }

    private static async Task<List<CotacaoLocalEntregaOption>> ReadLocaisEntregaInternalAsync(
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

    public async Task<CotacaoTabelaPrecoOption?> GetTabelaPrecoByClienteAsync(
        int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TblPrecoID, NmTblPreco
            FROM BrSupply..BR_TblPreco
            WHERE FlagAtivo = 1 AND ClienteID = @ClienteID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            return new CotacaoTabelaPrecoOption
            {
                TblPrecoId = reader.GetInt32(reader.GetOrdinal("TblPrecoID")),
                NmTblPreco = ReadString(reader, "NmTblPreco")
            };
        return null;
    }

    public async Task<int?> GetFormaPagamentoByClienteAsync(
        int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Cliente.FormaPagamentoSAP
            FROM BrSupply.dbo.BR_Cliente AS Cliente WITH (NOLOCK)
            WHERE Cliente.ClienteID = @ClienteID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is DBNull or null ? null : Convert.ToInt32(result);
    }

    public async Task<string?> GetTipoOVSAPByEnderecoAsync(
        int clienteEnderecoId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 ClienteEndereco.tipoOVSAP
            FROM BrSupply.dbo.BR_ClienteLocalEntrega AS ClienteLocalEntrega WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_ClienteEndereco AS ClienteEndereco WITH (NOLOCK)
                ON ClienteEndereco.ClienteEnderecoID = ClienteLocalEntrega.ClienteEnderecoID
            WHERE ClienteLocalEntrega.ClienteEnderecoID = @ClienteEnderecoID
              AND ClienteLocalEntrega.FlagAtivo = 1
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is DBNull or null ? null : result.ToString();
    }

    public async Task<IReadOnlyList<CotacaoSelectOption>> GetTiposOrdemAsync(
        int cotacaoTipoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT T.ID, T.Tipo AS CodTipoOV, T.Tipo + ' - ' + T.Descricao AS TipoOV
            FROM BrWeb..CotacaoTipo_OrdemVenda O (NOLOCK)
                JOIN Integracao_Clientes..BR_SAP_TiposDocumentosPedidos T (NOLOCK) ON T.Id = O.TipoDocumentoPedidoID
                JOIN BrWeb..Cotacao_Tipo C (NOLOCK) ON C.ID = O.CotacaoTipoID
                LEFT JOIN Integracao_Clientes.dbo.BR_SAP_RelacaoUsuariosTiposDocumentos RTD (NOLOCK) ON RTD.TipoId = T.ID
            WHERE O.CotacaoTipoID = @CotacaoTipoID AND RTD.UsuarioID = @UsuarioID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoTipoID", cotacaoTipoId);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

        var items = new List<CotacaoSelectOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoSelectOption
            {
                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                Nome = ReadString(reader, "TipoOV")
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoContratoOption>> GetContratosAsync(
        int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ClienteID, NrContrato, NrContrato + ' - ' + NmContrato AS Contrato
            FROM BrSupply.dbo.BR_ClienteGestaoContrato
            WHERE ClienteID = @ClienteID AND Vigencia >= CAST(GETDATE() AS DATE)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var items = new List<CotacaoContratoOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoContratoOption
            {
                NrContrato = ReadString(reader, "NrContrato"),
                Text       = ReadString(reader, "Contrato")
            });
        return items;
    }

    public async Task<IReadOnlyList<CotacaoSelectOption>> GetCidadesByUfAsync(
        string cdUf, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Cidade.CodigoIBGE, Cidade.NmCidade
            FROM BrSupply..BR_Cidade AS Cidade (NOLOCK)
            INNER JOIN BrSupply..BR_UF AS Estado (NOLOCK) ON Cidade.UFID = Estado.UFID AND Estado.CdUF = @CdUF
            WHERE Cidade.CidadeID > 0
            ORDER BY Cidade.NmCidade
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CdUF", cdUf);

        var items = new List<CotacaoSelectOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(new CotacaoSelectOption
            {
                Id   = reader.GetInt32(reader.GetOrdinal("CodigoIBGE")),
                Nome = ReadString(reader, "NmCidade")
            });
        return items;
    }

    public async Task<CotacaoEditDados?> GetPropostaParaEditAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                P.PropostaId, P.Nome, P.TipoID, P.TipoCotacao, P.EstabelecimentoID,
                P.ClienteId, P.ClienteEnderecoID, P.ClienteLocalEntregaID,
                P.ObsLocalEntrega, P.TabelaPrecoID, P.FlagPrecoConformeTabela,
                P.UfOrigem, P.UfDestino, P.CodigoIBGE, P.MargemPadrao,
                P.DataValidade, P.CondPagto, P.FormaPagamentoSAP, P.tipoOVSAP,
                P.OrdemCompra, P.NrContrato, P.TipoMotivoIDSAP,
                P.ContatoNome, P.ContatoEmail, P.Obs, P.StatusID,
                C.CdExtCliente + ' - ' + C.NmCliente AS ClienteNome,
                T.NmTblPreco,
                S.NmStatus AS StatusNome
            FROM BrWeb.dbo.Proposta P WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Cliente C WITH (NOLOCK) ON C.ClienteID = P.ClienteId
            LEFT JOIN BrSupply.dbo.BR_TblPreco T WITH (NOLOCK) ON T.TblPrecoID = P.TabelaPrecoID
            LEFT JOIN BrWeb.dbo.Proposta_Status S WITH (NOLOCK) ON S.StatusID = P.StatusID
            WHERE P.PropostaId = @PropostaId
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaId", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return new CotacaoEditDados
        {
            PropostaId              = Convert.ToInt32(reader["PropostaId"]),
            Nome                    = ReadString(reader, "Nome"),
            TipoCotacao             = ReadString(reader, "TipoCotacao"),
            EstabelecimentoID       = Convert.ToInt32(reader["EstabelecimentoID"]),
            ClienteId               = Convert.ToInt32(reader["ClienteId"]),
            ClienteNome             = ReadString(reader, "ClienteNome"),
            ClienteEnderecoID       = reader.IsDBNull(reader.GetOrdinal("ClienteEnderecoID")) ? null : Convert.ToInt32(reader["ClienteEnderecoID"]),
            ClienteLocalEntregaID   = reader.IsDBNull(reader.GetOrdinal("ClienteLocalEntregaID")) ? null : Convert.ToInt32(reader["ClienteLocalEntregaID"]),
            ObsLocalEntrega         = reader.IsDBNull(reader.GetOrdinal("ObsLocalEntrega")) ? null : ReadString(reader, "ObsLocalEntrega"),
            TabelaPrecoID           = reader.IsDBNull(reader.GetOrdinal("TabelaPrecoID")) ? null : Convert.ToInt32(reader["TabelaPrecoID"]),
            TabelaPrecoNome         = ReadString(reader, "NmTblPreco"),
            FlagPrecoConformeTabela = !reader.IsDBNull(reader.GetOrdinal("FlagPrecoConformeTabela")) && Convert.ToInt32(reader["FlagPrecoConformeTabela"]) == 1,
            UfOrigem                = ReadString(reader, "UfOrigem"),
            UfDestino               = ReadString(reader, "UfDestino"),
            CodigoIBGE              = reader.IsDBNull(reader.GetOrdinal("CodigoIBGE")) ? null : (int.TryParse(reader["CodigoIBGE"]?.ToString(), out var ibge) ? ibge : null),
            MargemPadrao            = reader.IsDBNull(reader.GetOrdinal("MargemPadrao")) ? null : Convert.ToDecimal(reader["MargemPadrao"]),
            DataValidade            = reader.IsDBNull(reader.GetOrdinal("DataValidade")) ? null : reader.GetDateTime(reader.GetOrdinal("DataValidade")),
            CondPagtoId             = reader.IsDBNull(reader.GetOrdinal("CondPagto")) ? null : Convert.ToInt32(reader["CondPagto"]),
            FormaPagamentoSAP       = reader.IsDBNull(reader.GetOrdinal("FormaPagamentoSAP")) ? null : Convert.ToInt32(reader["FormaPagamentoSAP"]),
            TipoOVSAP               = reader.IsDBNull(reader.GetOrdinal("tipoOVSAP")) ? null : ReadString(reader, "tipoOVSAP"),
            OrdemCompra             = reader.IsDBNull(reader.GetOrdinal("OrdemCompra")) ? null : ReadString(reader, "OrdemCompra"),
            NrContrato              = reader.IsDBNull(reader.GetOrdinal("NrContrato")) ? null : ReadString(reader, "NrContrato"),
            TipoMotivoIDSAP         = reader.IsDBNull(reader.GetOrdinal("TipoMotivoIDSAP")) ? null : Convert.ToInt32(reader["TipoMotivoIDSAP"]),
            ContatoNome             = reader.IsDBNull(reader.GetOrdinal("ContatoNome")) ? null : ReadString(reader, "ContatoNome"),
            ContatoEmail            = reader.IsDBNull(reader.GetOrdinal("ContatoEmail")) ? null : ReadString(reader, "ContatoEmail"),
            Obs                     = reader.IsDBNull(reader.GetOrdinal("Obs")) ? null : ReadString(reader, "Obs"),
            StatusID                = reader.IsDBNull(reader.GetOrdinal("StatusID")) ? 0 : Convert.ToInt32(reader["StatusID"]),
            StatusNome              = ReadString(reader, "StatusNome"),
        };
    }

    public async Task<(decimal Frete, decimal VlrPedidoMinimo)> BuscarFreteInicialAsync(
        int clienteEnderecoId, int clienteId, string? ufDestino, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sqlEndereco = """
            SELECT CONVERT(NUMERIC(10,2), ISNULL(VlrTaxaEntrega, 0)) AS VlrTaxaEntrega,
                   ISNULL(VlrPedidoMinimo, 0) AS VlrPedidoMinimo
            FROM BrSupply.dbo.BR_ClienteEndereco
            WHERE ClienteEnderecoID = @ClienteEnderecoID
            """;
        await using (var cmd = new SqlCommand(sqlEndereco, connection))
        {
            cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);
            await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await r.ReadAsync(cancellationToken))
            {
                var frete = r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                if (frete > 0) return (frete, min);
            }
        }

        const string sqlCliente = """
            SELECT ISNULL(VlrTaxaEntrega, 0) AS VlrTaxaEntrega,
                   ISNULL(VlrPedidoMinimo, 0) AS VlrPedidoMinimo
            FROM BrSupply.dbo.BR_Cliente
            WHERE ClienteID = @ClienteID
            """;
        await using (var cmd = new SqlCommand(sqlCliente, connection))
        {
            cmd.Parameters.AddWithValue("@ClienteID", clienteId);
            await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await r.ReadAsync(cancellationToken))
            {
                var frete = r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                if (frete > 0) return (frete, min);
            }
        }

        if (!string.IsNullOrWhiteSpace(ufDestino))
        {
            const string sqlCanal = """
                SELECT PF.VlrTaxaEntrega, ISNULL(PF.VlrPedidoMinimo, 0) AS VlrPedidoMinimo
                FROM BrSupply.dbo.BR_PoliticaFrete PF
                INNER JOIN BrSupply.dbo.BR_UF UF ON UF.UFID = PF.UFID
                WHERE PF.CanalVendaID = (SELECT CanalVendaID FROM BrSupply.dbo.BR_Cliente WHERE ClienteID = @ClienteID)
                  AND UF.CdUF = @UfDestino
                """;
            await using (var cmd = new SqlCommand(sqlCanal, connection))
            {
                cmd.Parameters.AddWithValue("@ClienteID", clienteId);
                cmd.Parameters.AddWithValue("@UfDestino", ufDestino);
                await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await r.ReadAsync(cancellationToken))
                {
                    var frete = r.IsDBNull(r.GetOrdinal("VlrTaxaEntrega")) ? 0m : r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                    var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                    if (frete > 0) return (frete, min);
                }
            }

            const string sqlUf = """
                SELECT PF.VlrTaxaEntrega, ISNULL(PF.VlrPedidoMinimo, 0) AS VlrPedidoMinimo
                FROM BrSupply.dbo.BR_PoliticaFrete PF
                INNER JOIN BrSupply.dbo.BR_UF UF ON UF.UFID = PF.UFID
                WHERE UF.CdUF = @UfDestino AND (PF.CanalVendaID IS NULL OR PF.CanalVendaID = 0)
                """;
            await using (var cmd = new SqlCommand(sqlUf, connection))
            {
                cmd.Parameters.AddWithValue("@UfDestino", ufDestino);
                await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await r.ReadAsync(cancellationToken))
                {
                    var frete = r.IsDBNull(r.GetOrdinal("VlrTaxaEntrega")) ? 0m : r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                    var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                    if (frete > 0) return (frete, min);
                }
            }
        }

        return (0m, 0m);
    }

    public async Task<CotacaoEmailTemplate?> GetDadosEmailTemplateAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Proposta.CdProposta                                          AS CdProposta,
                ISNULL(Proposta.OrdemCompra, '')                             AS OrdemCompra,
                ISNULL(Proposta.Obs, '')                                     AS Obs,
                ISNULL(Proposta.ContatoNome, '')                             AS ContatoNome,
                ISNULL(Proposta.ContatoEmail, '')                            AS ContatoEmail,
                CONVERT(VARCHAR(10), Proposta.DataValidade, 103)             AS DataValidade,
                ISNULL(CP.NmCondPagto, '')                                   AS CondPagtoNome,
                PropostaStatus.NmStatus                                      AS StatusNome,
                ISNULL(Proposta.DiasPrazoEntrega, 0)                         AS DiasPrazoEntrega,
                ISNULL(Transp.NmTransportadora, '')                          AS TransportadoraNome,
                ISNULL(Proposta.Frete, 0)                                    AS VlrFrete,
                (SELECT ISNULL(SUM(PI.VlrPrecoVenda),0)
                 FROM BrWeb..Proposta_Itens PI WITH (NOLOCK)
                 WHERE PI.PropostaID = Proposta.PropostaId)                  AS TotalVendaSemFrete,
                (SELECT ISNULL(SUM(PI.VlrPrecoVenda),0) + ISNULL(Proposta.Frete,0)
                 FROM BrWeb..Proposta_Itens PI WITH (NOLOCK)
                 WHERE PI.PropostaID = Proposta.PropostaId)                  AS TotalVendaFinal,
                ISNULL(Est.EstabelRazaoSocial, '')                           AS EstabRazaoSocial,
                ISNULL(Est.EstabelCNPJ, '')                                  AS EstabCNPJ,
                ISNULL(Est.InscrEstadual, '')                                AS EstabInscrEstadual,
                ISNULL(Est.EstabelTelefone, '')                              AS EstabTelefone,
                ISNULL(PARSENAME(REPLACE(Est.EstabelEndereco,',','.'),3),'') AS EstabEndereco,
                ISNULL(PARSENAME(REPLACE(Est.EstabelEndereco,',','.'),2),'') AS EstabNumero,
                ISNULL(PARSENAME(REPLACE(Est.EstabelEndereco,',','.'),1),'') AS EstabComplemento,
                ISNULL(Est.EstabelBairro, '')                                AS EstabBairro,
                ISNULL(CidEst.NmCidade, '')                                  AS EstabCidade,
                ISNULL(UFEst.CdUF, '')                                       AS EstabUF,
                ISNULL(Est.EstabelCEP, '')                                   AS EstabCEP,
                ISNULL(Consultor.NmUsuario, '')                              AS ConsultorNome,
                ISNULL(Consultor.Email, '')                                  AS ConsultorEmail,
                ISNULL(Consultor.Telefone, '')                               AS ConsultorTelefone,
                ISNULL(Cli.NmCliente, '')                                    AS ClienteRazaoSocial,
                ISNULL(Cli.CNPJCliente, '')                                  AS ClienteCNPJ,
                ISNULL(Cli.TelefoneCliente, '')                              AS ClienteTelefone,
                ISNULL(CLE.FlagEnderecoDiferente, 0)                         AS FlagEnderecoDiferente,
                ISNULL(CLE.DsLogradouro, '')                                 AS LocLogradouro,
                ISNULL(CLE.DsNumero, '')                                     AS LocNumero,
                ISNULL(CLE.DsComplemento, '')                                AS LocComplemento,
                ISNULL(CLE.DsBairro, '')                                     AS LocBairro,
                ISNULL(CidLoc.NmCidade, '')                                  AS LocCidade,
                ISNULL(UFLoc.CdUF, '')                                       AS LocUF,
                ISNULL(CLE.DsCEP, '')                                        AS LocCEP,
                ISNULL(CE.Logradouro, '')                                    AS EndLogradouro,
                ISNULL(CE.Numero, '')                                        AS EndNumero,
                ISNULL(CE.Complemento, '')                                   AS EndComplemento,
                ISNULL(CE.Bairro, '')                                        AS EndBairro,
                ISNULL(CE.Cidade, '')                                        AS EndCidade,
                ISNULL(UFEnd.CdUF, '')                                       AS EndUF,
                ISNULL(CE.CEP, '')                                           AS EndCEP
            FROM BrWeb.dbo.Proposta Proposta (NOLOCK)
            LEFT JOIN BrWeb.dbo.Proposta_Status PropostaStatus (NOLOCK)
                ON PropostaStatus.StatusID = Proposta.StatusID
            LEFT JOIN BrSupply.dbo.BR_CondPagto CP (NOLOCK)
                ON CP.CondPagtoID = Proposta.CondPagto
            LEFT JOIN BrSupply.dbo.BR_Transportadora Transp (NOLOCK)
                ON Transp.TransportadoraID = Proposta.TransportadoraID
            LEFT JOIN BrSupply.dbo.BR_Estabelecimento Est (NOLOCK)
                ON Est.EstabelecimentoID = Proposta.EstabelecimentoID
            LEFT JOIN BrSupply.dbo.BR_Cidade CidEst (NOLOCK)
                ON CidEst.CidadeID = Est.EstabelCidadeID
            LEFT JOIN BrSupply.dbo.BR_UF UFEst (NOLOCK)
                ON UFEst.UFID = Est.UFID
            LEFT JOIN BrSupply.dbo.BR_Usuario Consultor (NOLOCK)
                ON Consultor.UsuarioID = Proposta.UsuarioID
            LEFT JOIN BrSupply.dbo.BR_Cliente Cli (NOLOCK)
                ON Cli.ClienteID = Proposta.ClienteId
            LEFT JOIN BrSupply.dbo.BR_ClienteLocalEntrega CLE (NOLOCK)
                ON CLE.ClienteLocalEntregaID = Proposta.ClienteLocalEntregaID
            LEFT JOIN BrSupply.dbo.BR_Cidade CidLoc (NOLOCK)
                ON CidLoc.CidadeID = CLE.CdCidadeID
            LEFT JOIN BrSupply.dbo.BR_UF UFLoc (NOLOCK)
                ON UFLoc.UFID = CLE.CdUFID
            LEFT JOIN BrSupply.dbo.BR_ClienteEndereco CE (NOLOCK)
                ON CE.ClienteEnderecoID = Proposta.ClienteEnderecoID
            LEFT JOIN BrSupply.dbo.BR_UF UFEnd (NOLOCK)
                ON UFEnd.UFID = CE.UFID
            WHERE Proposta.PropostaId = @PropostaID
            """;

        const string itensSql = """
            SELECT
                PI.CodItemBR,
                PI.DescrItemBR,
                ISNULL(PI.PrecoItem, 0)      AS PrecoItem,
                ISNULL(PI.IPI, 0)            AS IPI,
                ISNULL(PI.ST, 0)             AS ST,
                ISNULL(PI.Quantidade, 0)     AS Quantidade,
                ISNULL(PI.VlrPrecoVenda / NULLIF(PI.Quantidade, 0), 0) AS VlrUnitario,
                ISNULL(Seg.NmSegmento, '')   AS NmSegmento,
                ISNULL(CF.CdClassificacaoFiscal, '') AS NCM
            FROM BrWeb.dbo.Proposta_Itens PI (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Item Item (NOLOCK)
                ON Item.CdItem = PI.CodItemBR
            LEFT JOIN BrSupply.dbo.BR_Segmento Seg (NOLOCK)
                ON Seg.SegmentoID = Item.SegmentoID
            LEFT JOIN BrSupply.dbo.BR_ClassificacaoFiscal CF (NOLOCK)
                ON CF.ClassificacaoFiscalID = Item.ClassificacaoFiscalID
            WHERE PI.PropostaID = @PropostaID
            ORDER BY PI.PropostaItemID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var flagEndDiferente = reader.IsDBNull(reader.GetOrdinal("FlagEnderecoDiferente"))
            ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("FlagEnderecoDiferente")));

        var dados = new CotacaoEmailTemplate
        {
            CdProposta          = ReadString(reader, "CdProposta"),
            OrdemCompra         = ReadString(reader, "OrdemCompra"),
            Obs                 = ReadString(reader, "Obs"),
            ContatoNome         = ReadString(reader, "ContatoNome"),
            ContatoEmail        = ReadString(reader, "ContatoEmail"),
            DataValidade        = ReadString(reader, "DataValidade"),
            CondPagtoNome       = ReadString(reader, "CondPagtoNome"),
            StatusNome          = ReadString(reader, "StatusNome"),
            DiasPrazoEntrega    = reader.IsDBNull(reader.GetOrdinal("DiasPrazoEntrega")) ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("DiasPrazoEntrega"))),
            TransportadoraNome  = ReadString(reader, "TransportadoraNome"),
            VlrFrete            = ReadDecimalField(reader, "VlrFrete"),
            TotalVendaSemFrete  = ReadDecimalField(reader, "TotalVendaSemFrete"),
            TotalVendaFinal     = ReadDecimalField(reader, "TotalVendaFinal"),
            EstabRazaoSocial    = ReadString(reader, "EstabRazaoSocial"),
            EstabCNPJ           = ReadString(reader, "EstabCNPJ"),
            EstabInscrEstadual  = ReadString(reader, "EstabInscrEstadual"),
            EstabTelefone       = ReadString(reader, "EstabTelefone"),
            EstabEndereco       = ReadString(reader, "EstabEndereco"),
            EstabNumero         = ReadString(reader, "EstabNumero"),
            EstabComplemento    = ReadString(reader, "EstabComplemento"),
            EstabBairro         = ReadString(reader, "EstabBairro"),
            EstabCidade         = ReadString(reader, "EstabCidade"),
            EstabUF             = ReadString(reader, "EstabUF"),
            EstabCEP            = ReadString(reader, "EstabCEP"),
            ConsultorNome       = ReadString(reader, "ConsultorNome"),
            ConsultorEmail      = ReadString(reader, "ConsultorEmail"),
            ConsultorTelefone   = ReadString(reader, "ConsultorTelefone"),
            ClienteRazaoSocial  = ReadString(reader, "ClienteRazaoSocial"),
            ClienteCNPJ         = ReadString(reader, "ClienteCNPJ"),
            ClienteTelefone     = ReadString(reader, "ClienteTelefone"),
            ClienteEndereco     = flagEndDiferente > 0 ? ReadString(reader, "LocLogradouro")  : ReadString(reader, "EndLogradouro"),
            ClienteNumero       = flagEndDiferente > 0 ? ReadString(reader, "LocNumero")      : ReadString(reader, "EndNumero"),
            ClienteComplemento  = flagEndDiferente > 0 ? ReadString(reader, "LocComplemento") : ReadString(reader, "EndComplemento"),
            ClienteBairro       = flagEndDiferente > 0 ? ReadString(reader, "LocBairro")      : ReadString(reader, "EndBairro"),
            ClienteCidade       = flagEndDiferente > 0 ? ReadString(reader, "LocCidade")      : ReadString(reader, "EndCidade"),
            ClienteUF           = flagEndDiferente > 0 ? ReadString(reader, "LocUF")          : ReadString(reader, "EndUF"),
            ClienteCEP          = flagEndDiferente > 0 ? ReadString(reader, "LocCEP")         : ReadString(reader, "EndCEP"),
        };

        await reader.CloseAsync();

        await using var cmdItens = new SqlCommand(itensSql, connection);
        cmdItens.Parameters.AddWithValue("@PropostaID", propostaId);

        var itens = new List<CotacaoEmailTemplateItem>();
        await using var readerItens = await cmdItens.ExecuteReaderAsync(cancellationToken);
        while (await readerItens.ReadAsync(cancellationToken))
            itens.Add(new CotacaoEmailTemplateItem
            {
                CodItemBR   = ReadString(readerItens, "CodItemBR"),
                DescrItemBR = ReadString(readerItens, "DescrItemBR"),
                PrecoItem   = ReadDecimalField(readerItens, "PrecoItem"),
                IPI         = ReadDecimalField(readerItens, "IPI"),
                ST          = ReadDecimalField(readerItens, "ST"),
                Quantidade  = ReadDecimalField(readerItens, "Quantidade"),
                VlrUnitario = ReadDecimalField(readerItens, "VlrUnitario"),
                NmSegmento  = ReadString(readerItens, "NmSegmento"),
                NCM         = ReadString(readerItens, "NCM"),
            });

        dados.Itens = itens;
        return dados;
    }

    private static decimal ReadDecimalField(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return 0m;
        return reader.GetFieldType(ordinal) == typeof(decimal)
            ? reader.GetDecimal(ordinal)
            : Convert.ToDecimal(reader.GetValue(ordinal));
    }
}
