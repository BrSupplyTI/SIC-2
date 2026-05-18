using Microsoft.Data.SqlClient;
using SIC.Web.Models.Cotacao;
using System.Text;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Serviço de consulta (somente leitura) para a listagem de cotações.
/// Usa ADO.NET direto, replicando o padrão de SqlPrePedidoPDFQueryRepository.
/// </summary>
public sealed class CotacaoQueryService(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    private const string BaseSql = """
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

    public async Task<IReadOnlyList<CotacaoListItemViewModel>> GetListAsync(
        CotacaoListFilterViewModel filtro,
        DateTime dataInicial,
        DateTime dataFinal,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(BaseSql, connection);

        cmd.Parameters.AddWithValue("@UsuarioID", filtro.UsuarioID ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FiltroCotacao", filtro.FiltroCotacao);
        cmd.Parameters.AddWithValue("@CdExtCliente", string.IsNullOrWhiteSpace(filtro.CdExtCliente) ? DBNull.Value : filtro.CdExtCliente);
        cmd.Parameters.AddWithValue("@PropostaID", filtro.PropostaId.HasValue ? filtro.PropostaId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@CNPJ", string.IsNullOrWhiteSpace(filtro.CNPJ) ? DBNull.Value : filtro.CNPJ);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", filtro.EstabelecimentoID.HasValue ? filtro.EstabelecimentoID.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@StatusID", filtro.StatusID.HasValue ? filtro.StatusID.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@DataInicial", dataInicial.Date);
        cmd.Parameters.AddWithValue("@DataFinal", dataFinal.Date.AddDays(1).AddSeconds(-1));

        var items = new List<CotacaoListItemViewModel>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapRow(reader));
        }

        return items;
    }

    public async Task<CotacaoViewModel?> GetByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(DetalheSql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var cotacao = MapDetalheToCotacaoViewModel(reader);
        await reader.CloseAsync();

        // Buscar status de crédito do cliente
        const string creditoSql = "SELECT BrSupply.dbo.fn_BR_ValidaCredito(@ClienteID, 0) AS StatusCredito";
        await using var cmdCredito = new SqlCommand(creditoSql, connection);
        cmdCredito.Parameters.AddWithValue("@ClienteID", cotacao.ClienteID);

        var statusCredito = await cmdCredito.ExecuteScalarAsync(cancellationToken);
        cotacao.StatusCredito = statusCredito?.ToString() ?? string.Empty;

        // Buscar dados do atendente (aprovação, margens, aprovador)
        if (cotacao.ConsultorUsuarioID > 0)
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
            cmdAtendente.Parameters.AddWithValue("@AtendenteID", cotacao.ConsultorUsuarioID);

            await using var readerAtendente = await cmdAtendente.ExecuteReaderAsync(cancellationToken);
            if (await readerAtendente.ReadAsync(cancellationToken))
            {
                cotacao.FlagPrecisaAprovacao    = readerAtendente.GetInt32(readerAtendente.GetOrdinal("FlagPrecisaAprovacao")) == 1;
                cotacao.PercMargemMinPedido     = ReadDecimal(readerAtendente, "PercMargemMinPedido");
                cotacao.PercMargemMaxPedido     = ReadDecimal(readerAtendente, "PercMargemMaxPedido");
                cotacao.AtendenteAprovadorID    = ReadInt(readerAtendente, "AprovadorID");
                cotacao.AtendenteAprovadorNome  = ReadString(readerAtendente, "AprovadorNmUsuario");
            }
        }

        cotacao.Itens = await GetItensByPropostaIdAsync(propostaId, cancellationToken);

        return cotacao;
    }

    public async Task<IReadOnlyList<SelectOptionViewModel>> GetCondicoesPagamentoAsync(
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

        var items = new List<SelectOptionViewModel>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SelectOptionViewModel
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nome = reader.GetString(reader.GetOrdinal("Nome")),
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<SelectOptionViewModel>> GetEstabelecimentoOptionsAsync(
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

        return await GetSelectOptionsAsync(sql, cancellationToken);
    }

    public async Task<IReadOnlyList<SelectOptionViewModel>> GetStatusOptionsAsync(
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

        return await GetSelectOptionsAsync(sql, cancellationToken);
    }

    private async Task<IReadOnlyList<SelectOptionViewModel>> GetSelectOptionsAsync(
        string sql,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        var items = new List<SelectOptionViewModel>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var idOrdinal = reader.GetOrdinal("Id");
            var nomeOrdinal = reader.GetOrdinal("Nome");

            items.Add(new SelectOptionViewModel
            {
                Id = reader.IsDBNull(idOrdinal) ? 0
                    : reader.GetFieldType(idOrdinal) == typeof(int)
                        ? reader.GetInt32(idOrdinal)
                        : Convert.ToInt32(reader.GetValue(idOrdinal)),
                Nome = reader.IsDBNull(nomeOrdinal) ? string.Empty : reader.GetString(nomeOrdinal),
            });
        }

        return items;
    }

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

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return string.Empty;

        var fieldType = reader.GetFieldType(ordinal);
        if (fieldType == typeof(string))
            return reader.GetString(ordinal);

        return reader.GetValue(ordinal)?.ToString() ?? string.Empty;
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
        catch
        {
            return 0m;
        }
    }

    private static CotacaoListItemViewModel MapRow(SqlDataReader reader) => new()
    {
        CdExtCliente = ReadString(reader, "Proposta__CdExtCliente"),
        PropostaId = ReadInt(reader, "Proposta__PropostaId"),
        CdProposta = ReadString(reader, "Proposta__CdProposta"),
        Nome = ReadString(reader, "Proposta__Nome"),
        DtCriacao = ReadString(reader, "Proposta__DtCriacao"),
        ClienteId = ReadInt(reader, "Proposta__ClienteId"),
        ClienteNome = ReadString(reader, "Cliente__Nome"),
        ClienteCNPJ = ReadString(reader, "Cliente__CNPJ"),
        MargemPadrao = ReadDecimal(reader, "Proposta__MargemPadrao"),
        Frete = ReadDecimal(reader, "Proposta__Frete"),
        DataValidade = ReadString(reader, "Proposta__DataValidade"),
        DataValidadeSQL = ReadString(reader, "Proposta__DataValidadeSQL"),
        StatusID = ReadInt(reader, "Proposta__StatusID"),
        StatusName = ReadString(reader, "Proposta__StatusName"),
        Obs = ReadString(reader, "Proposta__Obs"),
        NmMotivo = ReadString(reader, "Proposta__NmMotivo"),
        Justificativa = ReadString(reader, "Proposta__Justificativa"),
        CotacaoID = ReadNullableInt(reader, "Proposta__CotacaoID"),
        CotacaoStatusID = ReadNullableInt(reader, "Cotacao__StatusID"),
        CotacaoStatus = ReadString(reader, "Cotacao__Status"),
        TotalVenda = ReadDecimal(reader, "Proposta__TotalVenda"),
        TipoCotacao = ReadString(reader, "Proposta__TipoCotacao"),
        NmCondPagto = ReadString(reader, "Proposta__NmCondPagto"),
        Endereco = ReadString(reader, "Proposta__Endereco"),
        QtdItens = ReadInt(reader, "Proposta__QtdItens"),
        EstabelecimentoID = ReadNullableInt(reader, "Proposta__EstabelecimentoID"),
        NmEstabelecimento = ReadString(reader, "Proposta__NmEstabelecimento"),
        DataAbertura = ReadString(reader, "Proposta__DataAbertura"),
        DataAberturaSQL = ReadString(reader, "Proposta__DataAberturaSQL"),
        Executivo = ReadString(reader, "Proposta__Executivo"),
        AprovadorNmUsuario = ReadString(reader, "Aprovador__NmUsuario"),
    };

    private static bool ReadBool(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return false;

        var fieldType = reader.GetFieldType(ordinal);
        if (fieldType == typeof(bool))
            return reader.GetBoolean(ordinal);
        if (fieldType == typeof(int))
            return reader.GetInt32(ordinal) != 0;
        if (fieldType == typeof(byte))
            return reader.GetByte(ordinal) != 0;

        if (fieldType == typeof(string))
        {
            var stringValue = reader.GetString(ordinal).Trim().ToUpperInvariant();
            return stringValue switch
            {
                "SIM" or "S" or "TRUE" or "T" or "1" or "Y" or "YES" => true,
                "NAO" or "NÃO" or "N" or "FALSE" or "F" or "0" or "NO" => false,
                _ => false
            };
        }

        try
        {
            return Convert.ToBoolean(reader.GetValue(ordinal));
        }
        catch
        {
            return false;
        }
    }

    private static CotacaoViewModel MapDetalheToCotacaoViewModel(SqlDataReader reader) => new()
    {
        PropostaID = ReadInt(reader, "PropostaID"),
        CdProposta = ReadString(reader, "CdProposta"),
        Nome = ReadString(reader, "Nome"),
        Versao = ReadInt(reader, "Versao"),
        OrdemCompra = ReadString(reader, "OrdemCompra"),
        StatusID = ReadInt(reader, "StatusID"),
        StatusNome = ReadString(reader, "StatusNome"),
        TipoCotacao = ReadString(reader, "TipoCotacao"),
        DataValidade = ReadString(reader, "DataValidade"),
        EstabelecimentoID = ReadInt(reader, "EstabelecimentoID"),
        EstabelecimentoNome = ReadString(reader, "EstabelecimentoNome"),
        EstabelecimentoCNPJ = ReadString(reader, "EstabelecimentoCNPJ"),
        EstabelecimentoRazaoSocial = ReadString(reader, "EstabelecimentoRazaoSocial"),
        ClienteID = ReadInt(reader, "ClienteID"),
        ClienteCodigo = ReadString(reader, "ClienteCodigo"),
        ClienteNome = ReadString(reader, "ClienteNome"),
        ClienteCodNome = ReadString(reader, "ClienteCodNome"),
        ClienteCNPJ = ReadString(reader, "ClienteCNPJ"),
        ClienteContribuinte = ReadString(reader, "ClienteContribuinte"),
        EhContribuinte = ReadBool(reader, "EhContribuinte"),
        ClienteEnderecoID = ReadInt(reader, "ClienteEnderecoID"),
        ClienteEndereco = ReadString(reader, "ClienteEndereco"),
        ClienteCidadeEstado = ReadString(reader, "ClienteCidadeEstado"),
        ClienteLocalEntregaID = ReadInt(reader, "ClienteLocalEntregaID"),
        LocalEntregaNome = ReadString(reader, "LocalEntregaNome"),
        LocalEntregaEndereco = ReadString(reader, "LocalEntregaEndereco"),
        LocalEntregaCidadeEstado = ReadString(reader, "LocalEntregaCidadeEstado"),
        LocalEntregaObservacao = ReadString(reader, "LocalEntregaObservacao"),
        CanalVenda = ReadString(reader, "CanalVenda"),
        TipoOrdem = ReadString(reader, "TipoOrdem"),
        TipoOVSAP = ReadString(reader, "TipoOVSAP"),
        TipoOVEhRevenda = ReadBool(reader, "TipoOVEhRevenda"),
        TipoMotivoIDSAP = ReadNullableInt(reader, "TipoMotivoIDSAP"),
        Motivo = ReadString(reader, "Motivo"),
        MotivoNome = ReadString(reader, "MotivoNome"),
        Justificativa = ReadString(reader, "Justificativa"),
        AprovadorUsuarioID = ReadNullableInt(reader, "AprovadorUsuarioID"),
        AprovadorNome = ReadString(reader, "AprovadorNome"),
        AprovadorJustificativa = ReadString(reader, "AprovadorJustificativa"),
        CondPagtoID = ReadNullableInt(reader, "CondPagtoID"),
        CondPagtoNome = ReadString(reader, "CondPagtoNome"),
        FormaPagamentoSAP = ReadNullableInt(reader, "FormaPagamentoSAP"),
        FormaPagamentoDesc = ReadString(reader, "FormaPagamentoDesc"),
        FlagDefCondPagTelevendas = ReadBool(reader, "FlagDefCondPagTelevendas"),
        TabelaPrecoID = ReadString(reader, "TabelaPrecoID"),
        TabelaPrecoNome = ReadString(reader, "TabelaPrecoNome"),
        FlagPrecoConformeTabela = ReadBool(reader, "FlagPrecoConformeTabela"),
        MargemPadrao = ReadDecimal(reader, "MargemPadrao"),
        MargemBruta = ReadDecimal(reader, "MargemBruta"),
        MargemContribuida = ReadDecimal(reader, "MargemContribuida"),
        MargemBrutaFixa = ReadDecimal(reader, "MargemBrutaFixa"),
        MargemContribuidaFixa = ReadDecimal(reader, "MargemContribuidaFixa"),
        Frete = ReadString(reader, "Frete"),
        ValorVendaTotal = ReadDecimal(reader, "ValorVendaTotal"),
        VlrContribTotal = ReadDecimal(reader, "VlrContribTotal"),
        ValorContribuicaoFixo = ReadDecimal(reader, "ValorContribuicaoFixo"),
        ValorTotalFixo = ReadDecimal(reader, "ValorTotalFixo"),
        VlrPedidoMinimo = ReadDecimal(reader, "VlrPedidoMinimo"),
        TotalVenda = ReadString(reader, "TotalVenda"),
        TotalVendaFrete = ReadString(reader, "TotalVendaFrete"),
        TotalVendaSemImposto = ReadString(reader, "TotalVendaSemImposto"),
        TotalVendaFreteSemImposto = ReadString(reader, "TotalVendaFreteSemImposto"),
        TotalPeso = ReadDecimal(reader, "TotalPeso"),
        QtdItens = ReadInt(reader, "QtdItens"),
        DiasPrazoEntrega = ReadInt(reader, "DiasPrazoEntrega"),
        DataProgEntrega = ReadString(reader, "DataProgEntrega"),
        NatOperacao = ReadString(reader, "NatOperacao"),
        UfOrigem = ReadString(reader, "UfOrigem"),
        UfDestino = ReadString(reader, "UfDestino"),
        CodigoIBGE = ReadString(reader, "CodigoIBGE"),
        ContatoNome = ReadString(reader, "ContatoNome"),
        ContatoEmail = ReadString(reader, "ContatoEmail"),
        TransportadoraID = ReadNullableInt(reader, "TransportadoraID"),
        TransportadoraNome = ReadString(reader, "TransportadoraNome"),
        CotacaoID = ReadNullableInt(reader, "CotacaoID"),
        CotacaoIdOriginal = ReadNullableInt(reader, "CotacaoIdOriginal"),
        CotacaoStatusDesc = ReadString(reader, "CotacaoStatusDesc"),
        CotacaoEnvioComentarios = ReadString(reader, "CotacaoEnvioComentarios"),
        FlagRevisarValorProdutos = ReadBool(reader, "FlagRevisarValorProdutos"),
        FlagRevisarValorFrete = ReadBool(reader, "FlagRevisarValorFrete"),
        FlagRevisarPrazoPagamento = ReadBool(reader, "FlagRevisarPrazoPagamento"),
        FlagRevisarPrazoEntrega = ReadBool(reader, "FlagRevisarPrazoEntrega"),
        FlagRevisarAtendimento = ReadBool(reader, "FlagRevisarAtendimento"),
        FlagRevisarPermiteTrocarMarca = ReadBool(reader, "FlagRevisarPermiteTrocarMarca"),
        FlagRevisarPermiteTrocarUnidade = ReadBool(reader, "FlagRevisarPermiteTrocarUnidade"),
        FlagPrecosInformados = ReadBool(reader, "FlagPrecosInformados"),
        CotacaoEnvioIPAprovacao = ReadString(reader, "CotacaoEnvioIPAprovacao"),
        ConsultorUsuarioID = ReadNullableInt(reader, "ConsultorUsuarioID"),
        ConsultorNome = ReadString(reader, "ConsultorNome"),
        ConsultorEmail = ReadString(reader, "ConsultorEmail"),
        CarteiraNome = ReadString(reader, "CarteiraNome"),
        Observacao = ReadString(reader, "Observacao"),
        Obs = ReadString(reader, "Obs"),
    };

    public async Task<IReadOnlyList<CotacaoItemViewModel>> GetItensByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(PropostaItensSql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var items = new List<CotacaoItemViewModel>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapItemToCotacaoItemViewModel(reader));
        }

        return items;
    }

    private static CotacaoItemViewModel MapItemToCotacaoItemViewModel(SqlDataReader reader)
    {
        var quantidade      = ReadDecimal(reader, "PropostaItem__Quantidade");
        var precoItem       = ReadDecimal(reader, "PropostaItem__PrecoItem");
        var vlrPrecoVenda   = ReadDecimal(reader, "PropostaItem__VlrPrecoVenda");
        var valorIcms       = ReadDecimal(reader, "PropostaItem__ValorICMS");
        var ipi             = ReadDecimal(reader, "PropostaItem__IPI");
        var st              = ReadDecimal(reader, "PropostaItem__ST");

        var totalSemImposto = precoItem * quantidade;
        var totalImpostos   = valorIcms + ipi + st;

        var tipoCusto            = ReadString(reader, "PropostaItem__TipoCusto");
        var vlrCustoAquisicao     = ReadDecimal(reader, "PropostaItem__VlrCustoAquisicao");
        var vlrCustoMedio         = ReadDecimal(reader, "PropostaItem__VlrCustoMedio");
        var custoLiquido          = tipoCusto == "M" ? vlrCustoMedio : vlrCustoAquisicao;

        return new CotacaoItemViewModel
        {
            PropostaItemID         = ReadInt(reader, "PropostaItem__PropostaItemID"),
            PropostaID             = ReadInt(reader, "PropostaItem__PropostaID"),
            ProdutoID              = ReadNullableInt(reader, "PropostaItem__ItemID"),
            CodigoProduto          = ReadString(reader, "PropostaItem__CodItemBR"),
            DescricaoProduto       = ReadString(reader, "PropostaItem__DescrItemBR"),
            UnidadeMedida          = ReadString(reader, "PropostaItem__UniMedBr"),
            UnidadeMedidaDescricao = ReadString(reader, "PropostaItem__UniMedBr"),
            Quantidade             = quantidade,
            Peso                   = 0,
            EstoqueDisponivel      = ReadDecimal(reader, "PropostaItem__QtEstoqueSIC"),
            PrecoMinimo            = ReadDecimal(reader, "PropostaItem__PrecoMinimo"),
            PrecoTabelaPreco       = ReadDecimal(reader, "PropostaItem__PrecoTabela"),
            TipoCusto              = string.IsNullOrWhiteSpace(tipoCusto) ? "A" : tipoCusto,
            VlrCustoAquisicao      = vlrCustoAquisicao,
            VlrCustoMedio          = vlrCustoMedio,
            CustoLiquido           = custoLiquido,
            PrecoItem              = precoItem,
            PrecoUnitario          = ReadDecimal(reader, "PropostaItem__PrecoItem"),
            VlrPrecoVenda          = vlrPrecoVenda,
            Margem                 = ReadDecimal(reader, "PropostaItem__MargemCalculada"),
            MargemPercentual       = ReadDecimal(reader, "PropostaItem__Margem"),
            ICMS                   = ReadDecimal(reader, "PropostaItem__ICM"),
            IPI                    = ipi,
            ST                     = st,
            PIS                    = ReadDecimal(reader, "PropostaItem__Pis"),
            COFINS                 = ReadDecimal(reader, "PropostaItem__Cofins"),
            TotalImpostos          = totalImpostos,
            TotalSemImposto        = totalSemImposto,
            TotalComImposto        = vlrPrecoVenda,
            // ── Impostos detalhados (PDF) ──
            ValorLiqUnit               = ReadDecimal(reader, "PropostaItem__ValorLiqUnit"),
            ValorICMS                  = valorIcms,
            PercIPI                    = ReadDecimal(reader, "PropostaItem__PercIPI"),
            ValorFundoCombPobreza      = ReadDecimal(reader, "PropostaItem__ValorFundoCombPobreza"),
            ValorPis                   = ReadDecimal(reader, "PropostaItem__ValorPis"),
            ValorCOFINS                = ReadDecimal(reader, "PropostaItem__ValorCOFINS"),
            ValorFCPST                 = ReadDecimal(reader, "PropostaItem__ValorFCPST"),
            ValorICMSPartilhaOrigem    = ReadDecimal(reader, "PropostaItem__ValorICMSPartilhaOrigem"),
            ValorICMSPartilhaDestino   = ReadDecimal(reader, "PropostaItem__ValorICMSPartilhaDestino"),
            // ── Classificação fiscal (PDF) ──
            NCM                    = ReadString(reader, "PropostaItem__NCM"),
            NumCA                  = ReadString(reader, "PropostaItem__NumCA"),
            SegmentoID             = ReadInt(reader, "PropostaItem__SegmentoID"),
            NmSegmento             = ReadString(reader, "PropostaItem__NmSegmento"),
            NmFamilia              = ReadString(reader, "PropostaItem__NmFamilia"),
            NmSubFamilia           = ReadString(reader, "PropostaItem__NmSubFamilia"),
            CodBarras              = ReadString(reader, "PropostaItem__CodBarras"),
            MVA                    = ReadDecimal(reader, "PropostaItem__MVA"),
            Marca                  = string.Empty,
            Fornecedor             = string.Empty,
            Observacao             = string.Empty,
            PermiteTrocarMarca     = false,
            PermiteTrocarUnidade   = false,
            FlagAtivo              = true,
            NumeroLinha            = ReadNullableInt(reader, "PropostaItem__Numero"),
        };
    }

    /// <summary>
    /// Calcula as opções de frete para uma proposta usando a função Fn_Calcula_Fretes_Proposta.
    /// </summary>
    public async Task<IReadOnlyList<FreteOpcaoViewModel>> CalcularFretePropostaAsync(int propostaId, CancellationToken cancellationToken)    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM BrSupply.dbo.Fn_Calcula_Fretes_Proposta(@PropostaID)";
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var list = new List<FreteOpcaoViewModel>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new FreteOpcaoViewModel
            {
                TransportadoraID = ReadInt(reader, "TransportadoraID"),
                Nome = ReadString(reader, "Nome"),
                TempoLogistico = ReadInt(reader, "TempoLogistico"),
                TempoComercial = ReadInt(reader, "TempoComercial"),
                TaxaExtra = ReadDecimal(reader, "TaxaExtra"),
                ValorFrete = ReadDecimal(reader, "ValorFrete"),
                QtItensRestritos = ReadInt(reader, "QtItensRestritos"),
                FlagObrigatoriaCanalVenda = ReadBool(reader, "FlagObrigatoriaCanalVenda"),
                FlagClienteRestrito = ReadBool(reader, "FlagClienteRestrito"),
                FlagClienteFixo = ReadBool(reader, "FlagClienteFixo"),
            });
        }

        return list;
    }

    /// <summary>
    /// Finaliza a proposta, definindo StatusID = 2 (Finalizado) ou StatusID = 10 (Aguarda Aprovação)
    /// caso o atendente precise de aprovação e a margem bruta esteja fora do intervalo permitido.
    /// Retorna o StatusID resultante, ou null em caso de falha.
    /// </summary>
    public async Task<int?> FinalizarAsync(
        int propostaId,
        string dataValidade,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var statusId = 2;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 1. Busca dados do atendente (aprovação e margens)
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

                    // 2. Busca MargemBruta da proposta
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
                        {
                            statusId = 10;
                        }
                    }
                }
            }
        }

        // 3. Atualiza a proposta
        const string updateSql = """
            UPDATE BrWeb.dbo.Proposta
            SET DataValidade = @DataValidade,
                StatusID     = @StatusID,
                UsuarioID    = @UsuarioID
            WHERE PropostaId = @PropostaID
            """;

        await using var cmdUpdate = new SqlCommand(updateSql, connection);
        cmdUpdate.Parameters.AddWithValue("@DataValidade", dataValidade);
        cmdUpdate.Parameters.AddWithValue("@StatusID", statusId);
        cmdUpdate.Parameters.AddWithValue("@UsuarioID", usuarioId);
        cmdUpdate.Parameters.AddWithValue("@PropostaID", propostaId);

        var rows = await cmdUpdate.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0 ? statusId : null;
    }

    public async Task AprovarAsync(
        int propostaId,
        int aprovadorId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BrWeb.dbo.Proposta
            SET StatusID           = 2,
                AprovadorUsuarioID = @AprovadorID
            WHERE PropostaId = @PropostaID
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@AprovadorID", aprovadorId);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ReprovarAsync(
        int propostaId,
        int aprovadorId,
        string justificativa,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BrWeb.dbo.Proposta
            SET StatusID               = 1,
                AprovadorUsuarioID     = @AprovadorID,
                JustificativaAprovador = @Justificativa
            WHERE PropostaId = @PropostaID
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@AprovadorID", aprovadorId);
        cmd.Parameters.AddWithValue("@Justificativa", justificativa);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SalvarFretePropostaAsync(
        int propostaId,
        int transportadoraId,
        decimal valorFrete,
        int prazoTotal,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BrWeb.dbo.Proposta
            SET TransportadoraID   = @TransportadoraID,
                Frete              = @Frete,
                DiasPrazoEntrega   = @DiasPrazoEntrega
            WHERE PropostaId = @PropostaID
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@TransportadoraID", transportadoraId);
        cmd.Parameters.AddWithValue("@Frete", valorFrete);
        cmd.Parameters.AddWithValue("@DiasPrazoEntrega", prazoTotal);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<CotacaoItemImpostosViewModel?> GetImpostosItemAsync(
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

        return new CotacaoItemImpostosViewModel
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

    /// <summary>
    /// Retorna os itens de uma proposta validados via BR_SP_ValidaItensProposta,
    /// usados para cruzar com os dados do Excel na importação.
    /// </summary>
    public async Task<IReadOnlyList<CotacaoItemValidacaoViewModel>> ValidarItensImportacaoAsync(
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

        var items = new List<CotacaoItemValidacaoViewModel>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CotacaoItemValidacaoViewModel
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

    public async Task<int?> AutorizarFaturamentoAsync(
        int propostaId,
        string ipAprovacao,
        CancellationToken cancellationToken = default)
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
            cmd.Parameters.AddWithValue("@PropostaID", propostaId);
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
        return result is DBNull or null ? null : Convert.ToInt32(result);
    }

    /// <summary>
    /// Retorna os dados consolidados para a tela EnviarEmailCotacao.
    /// Combina a consulta principal (Proposta) com a consulta auxiliar de dados resumidos do cliente.
    /// </summary>
    public async Task<EnviarEmailCotacaoViewModel?> GetEnviarEmailDadosAsync(
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

                (
                    ISNULL(Cliente.CdExtCliente, '')
                    + ' - '
                    + ISNULL(Cliente.NmCliente, '')
                )                                                               AS ClienteNome,

                (
                    ISNULL(ClienteEndereco.Cidade, '')
                    + ' - '
                    + ISNULL(UF.CdUF, '')
                )                                                               AS ClienteCidadeEstado,

                Proposta.ContatoNome                                            AS ContatoNome,
                Proposta.ContatoEmail                                           AS ContatoEmail,

                Consultor.NmUsuario                                             AS ConsultorNome,
                Consultor.Email                                                 AS ConsultorEmail,

                Executivo.NmUsuario                                             AS ExecutivoNome,
                Executivo.Email                                                 AS ExecutivoEmail,

                FORMAT(
                    (SELECT SUM(PItem.VlrPrecoVenda)
                     FROM BrWeb..Proposta_Itens AS PItem WITH (NOLOCK)
                     WHERE PItem.PropostaID = Proposta.PropostaId),
                    'C', 'pt-BR'
                )                                                               AS TotalVenda,

                FORMAT(ISNULL(Proposta.Frete, 0), 'C', 'pt-BR')                AS Frete

            FROM BrWeb.dbo.Proposta Proposta (NOLOCK)

                LEFT JOIN BrSupply.dbo.BR_Estabelecimento Estabelecimento (NOLOCK)
                    ON Estabelecimento.EstabelecimentoID = Proposta.EstabelecimentoID

                LEFT JOIN BrSupply.dbo.BR_Usuario Consultor (NOLOCK)
                    ON Consultor.UsuarioID = Proposta.UsuarioID

                LEFT JOIN BrSupply.dbo.BR_Cliente Cliente (NOLOCK)
                    ON Cliente.ClienteID = Proposta.ClienteId

                    LEFT JOIN BrSupply.dbo.BR_Carteira Carteira (NOLOCK)
                        ON Carteira.CarteiraID = Cliente.CarteiraID

                        LEFT JOIN BrSupply.dbo.BR_Usuario Executivo (NOLOCK)
                            ON Executivo.UsuarioID = Carteira.ExecVendasID

                LEFT JOIN BrSupply.dbo.BR_ClienteEndereco ClienteEndereco (NOLOCK)
                    ON ClienteEndereco.ClienteEnderecoID = Proposta.ClienteEnderecoID

                    LEFT JOIN BrSupply.dbo.BR_UF UF (NOLOCK)
                        ON UF.UFID = ClienteEndereco.UFID

            WHERE Proposta.PropostaId = @PropostaID

            ORDER BY Proposta.PropostaId DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new EnviarEmailCotacaoViewModel
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

    /// <summary>
    /// Retorna o histórico de envios da cotação.
    /// Fonte: BRWeb..Proposta_CotacaoEnvio JOIN BrSupply..BR_Usuario
    /// </summary>
    public async Task<IReadOnlyList<CotacaoEnvioHistoricoItemViewModel>> GetHistoricoEnviosAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                E.PropostaCotacaoEnvioID,
                E.Nome,
                E.Email,

                CONVERT(VARCHAR(10), E.DataHora, 103)
                    + ' '
                    + CONVERT(VARCHAR(5), E.DataHora, 108)              AS DtEnvio,

                U.NmUsuario,

                CONVERT(VARCHAR(10), E.DataHoraVisualizacao, 103)
                    + ' '
                    + CONVERT(VARCHAR(5), E.DataHoraVisualizacao, 108)  AS DtVisualizacao,

                CASE FlagVisualizaEstoque
                    WHEN 0 THEN 'N'
                    ELSE 'S'
                END                                                     AS FlagVisualizaEstoque,

                CASE FlagPodeNegociar
                    WHEN 0 THEN 'N'
                    ELSE 'S'
                END                                                     AS FlagPodeNegociar,

                CASE FlagPodetrocartransportadora
                    WHEN 0 THEN 'N'
                    ELSE 'S'
                END                                                     AS FlagPodeTrocarTransportadora,

                CASE FlagPodeTrocarCondPagto
                    WHEN 0 THEN 'N'
                    ELSE 'S'
                END                                                     AS FlagPodeTrocarCondPagto,

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

        var items = new List<CotacaoEnvioHistoricoItemViewModel>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CotacaoEnvioHistoricoItemViewModel
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
}

