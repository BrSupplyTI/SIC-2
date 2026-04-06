using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;
using System.Data;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlProductCatalogRepository(IConfiguration configuration) : IProductCatalogRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<ProductCatalogItem>> GetCatalogAsync(
        int pageNumber,
        int pageSize,
        string? comecaComTexto,
        string? contemTexto,
        int flagAtivo,
        int flagMarcaPropria,
        int estabelecimentoId,
        int flagOutlet,
        int flagSobDemanda,
        int flagSustentavel,
        int flagNovidade,
        string? curva,
        int flagPadraoBrSupply,
        int flagComEstoque,
        string? orderBy,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_ProdutosCatalogo", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
        cmd.Parameters.Add("@ComecaComTexto", SqlDbType.VarChar, 100).Value = comecaComTexto ?? string.Empty;
        cmd.Parameters.Add("@ContemTexto", SqlDbType.VarChar, 100).Value = contemTexto ?? string.Empty;
        cmd.Parameters.Add("@FlagAtivo", SqlDbType.Int).Value = flagAtivo;
        cmd.Parameters.Add("@FlagMarcaPropria", SqlDbType.Int).Value = flagMarcaPropria;
        cmd.Parameters.Add("@EstabelecimentoID", SqlDbType.Int).Value = estabelecimentoId;
        cmd.Parameters.Add("@FlagOutlet", SqlDbType.Int).Value = flagOutlet;
        cmd.Parameters.Add("@FlagSobSemanda", SqlDbType.Int).Value = flagSobDemanda;
        cmd.Parameters.Add("@FlagSustentavel", SqlDbType.Int).Value = flagSustentavel;
        cmd.Parameters.Add("@FlagNovidade", SqlDbType.Int).Value = flagNovidade;
        cmd.Parameters.Add("@Curva", SqlDbType.VarChar, 1).Value = curva ?? string.Empty;
        cmd.Parameters.Add("@FlagPadraoBrSupply", SqlDbType.Int).Value = flagPadraoBrSupply;
        cmd.Parameters.Add("@FlagComEstoque", SqlDbType.Int).Value = flagComEstoque;
        cmd.Parameters.Add("@OrderBy", SqlDbType.VarChar, 50).Value = orderBy ?? "Nome (A-Z)";

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ProductCatalogItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductCatalogItem
            {
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                CdItem = ReadString(reader, "CdItem"),
                NmItem = ReadString(reader, "NmItem"),
                NmSegmento = ReadString(reader, "NmSegmento"),
                NmFamilia = ReadString(reader, "NmFamilia"),
                NmSubFamilia = ReadString(reader, "NmSubFamilia"),
                NmMarca = ReadString(reader, "NmMarca"),
                FlagTipoMarca = ReadString(reader, "FlagTipoMarca"),
                NumCA = ReadNullableString(reader, "NumCA"),
                QtEstoque = ReadNullableInt32(reader, "QtEstoque") ?? 0,
                Curva = ReadString(reader, "Curva"),
                DtCadastro = ReadNullableDateTime(reader, "DtCadastro"),
                TotalRegistros = reader.GetInt32(reader.GetOrdinal("TotalRegistros"))
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<CatalogEstablishment>> GetEstablishmentsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 0 AS EstabelecimentoID,
                   'Todos os Estabelecimentos' AS NmEstabelecimento,
                   0 AS OrdemExibicao
            UNION ALL
            SELECT E.EstabelecimentoID,
                   E.NmEstabelecimento,
                   E.OrdemExibicao
            FROM BR_Estabelecimento E WITH (NOLOCK)
            WHERE E.FlagAtivo = 1
              AND ISNULL(E.NmCurto,'') <> ''
              AND SerieNF > 0
            ORDER BY OrdemExibicao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<CatalogEstablishment>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CatalogEstablishment
            {
                EstabelecimentoID = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                NmEstabelecimento = ReadString(reader, "NmEstabelecimento")
            });
        }

        return items;
    }

    public async Task<ProductDetail?> GetProductDetailAsync(int itemId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DECLARE @Origem VARCHAR(20) = ''
            DECLARE @OutletMax INT = 0
            DECLARE @OutletMin INT = 0

            SELECT TOP 1 @Origem = 
                    CASE
                        TI.TipoProduto
                        WHEN 1 THEN 'Importado'
                        WHEN 2 THEN 'Importado'
                        ELSE 'Nacional'
                    END    
            FROM Integracao_Clientes..SRM_FornecedorTblPreco T WITH (NOLOCK)
            JOIN Integracao_Clientes..SRM_FornecedorTblPrecoItem TI WITH (NOLOCK) ON TI.FornecedorTblprecoID = T.FornecedorTblPrecoID
            WHERE T.DtTermino >= GETDATE() - 365    
              AND TI.ItemID = @ItemID

            
            SELECT @OutletMax = MAX(ISNULL(E.FlagOutlet,0)),
                   @OutletMin = MIN(ISNULL(E.FlagOutLet,0))
            FROM BR_PrecoEstoque E WITH (NOLOCK)
            JOIN BR_Estabelecimento ES WITH (NOLOCK) ON ES.EstabelecimentoID = E.EstabelecimentoID
            WHERE E.ItemID = @ItemID
            AND ISNULL(ES.NmCurto,'') <> ''

            SELECT TOP 1
                I.ItemID,
                I.CdItem,
                I.NmItem,
                I.SegmentoID,
                S.NmSegmento,
                I.FamiliaID,
                F.NmFamilia,
                I.SubFamiliaID,
                U.NmSubFamilia,
                M.NmMarca,
                I.DsBula AS DescricaoLonga,
                ISNULL(I.TituloDsInformacaoTecnica,'Informações Técnicas') AS TituloDsInformacaoTecnica,
                I.DsInformacaoTecnica AS InformacaoTecnica,
                ISNULL(I.QtMultiplicador, 0) AS QtMultiplicador,
                ISNULL(I.QtMultiplicadorLiberado, 0) AS QtMultiplicadorLiberado,
                CONVERT(DECIMAL(10, 2), I.NrPeso) AS NrPeso,
                CASE WHEN I.DtMensagem > GETDATE()
                    THEN DsMensagem
                    ELSE ''
                END AS Mensagem,
                I.DtMensagem,
                CASE M.FlagTipoMarca
                    WHEN 'Marca Própria' THEN 1
                    ELSE 0
                END AS FlagMarcaPropria,
                S.ImgSegmentoMini AS IconeSegmento,
                S.FlagAtivo AS FlagAtivoSegmento,
                I.Tags,
                I.NumCA,
                CA.Validade AS ValidadeCA,
                ISNULL(I.FlagLancamento, 0) AS FlagLancamento,
                ISNULL(I.FlagSustentavel, 0) AS FlagSustentavel,
                I.CdUnidade,
                I.QtdEmbalagem,
                E.NmEmbalagem,
                T.Sigla AS UnidadeMedida,
                I.QtdeCaixaMaster,
                W.Cod_Barras AS CodigoBarras,
                I.CodDUN,
                ISNULL(I.FlagFaltaNoFabricante, 0) AS FlagFaltaNoFabricante,
                I.FlagAtivo,
                ISNULL(I.FlagCatalogo, 0) AS FlagCatalogo,
                SUBSTRING(C.CdClassificacaoFiscal, 1, 8) AS CdClassificacaoFiscal,
                I.Modelo,
                I.Normas,
                I.Referencia,
                I.FSC,
                I.ABNT,
                I.Anatel,
                I.Anvisa,
                I.Inmetro,
                I.FlagDualSourcing,
                I.DtCadastro,
                @Origem AS Origem,
                IIF(@OutletMax <> @OutletMin, 2, @OutletMax) AS FlagOutlet
            FROM BR_Item I WITH (NOLOCK)
            JOIN BR_Segmento S WITH (NOLOCK) ON S.SegmentoID = I.SegmentoID
            JOIN BR_Familia F WITH (NOLOCK) ON F.FamiliaID = I.FamiliaID
            JOIN BR_SubFamilia U WITH (NOLOCK) ON U.SubFamiliaID = I.SubFamiliaID
            LEFT JOIN BR_ProdutoMarca M WITH (NOLOCK) ON M.[ProdutoMarcaID] = I.[ProdutoMarcaID]
            LEFT JOIN BR_Embalagem E WITH (NOLOCK) ON E.EmbalagemID = I.EmbalagemID
            LEFT JOIN BR_UnidadeItem T WITH (NOLOCK) ON T.UnidadeItemID = I.UnidadeItemID
            LEFT JOIN BR_ClassificacaoFiscal C WITH (NOLOCK) ON C.ClassificacaoFiscalID = I.ClassificacaoFiscalID
            LEFT JOIN BR_NumCA CA (NOLOCK) ON CA.NumCA = I.NumCA
            LEFT JOIN BrWeb..Preco_Itens W WITH (NOLOCK) ON W.Produto = I.CdItem
            WHERE I.ItemID = @ItemID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var detail = new ProductDetail
        {
            ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
            CdItem = ReadString(reader, "CdItem"),
            NmItem = ReadString(reader, "NmItem"),
            SegmentoID = reader.GetInt32(reader.GetOrdinal("SegmentoID")),
            NmSegmento = ReadString(reader, "NmSegmento"),
            FamiliaID = reader.GetInt32(reader.GetOrdinal("FamiliaID")),
            NmFamilia = ReadString(reader, "NmFamilia"),         
            SubFamiliaID = reader.GetInt32(reader.GetOrdinal("SubFamiliaID")),
            NmSubFamilia = ReadString(reader, "NmSubFamilia"),
            NmMarca = ReadString(reader, "NmMarca"),
            DescricaoLonga = ReadString(reader, "DescricaoLonga"),
            TituloDsInformacaoTecnica = ReadString(reader, "TituloDsInformacaoTecnica"),
            InformacaoTecnica = ReadString(reader, "InformacaoTecnica"),
            QtMultiplicador = reader.GetInt32(reader.GetOrdinal("QtMultiplicador")),
            QtMultiplicadorLiberado = reader.GetInt32(reader.GetOrdinal("QtMultiplicadorLiberado")),
            NrPeso = ReadDecimal(reader, "NrPeso"),
            Mensagem = ReadString(reader, "Mensagem"),
            DtMensagem = ReadNullableDateTime(reader, "DtMensagem"),
            DtCadastro = ReadNullableDateTime(reader, "DtCadastro"),
            FlagMarcaPropria = reader.GetInt32(reader.GetOrdinal("FlagMarcaPropria")),
            IconeSegmento = ReadString(reader, "IconeSegmento"),
            FlagAtivoSegmento = reader.GetInt32(reader.GetOrdinal("FlagAtivoSegmento")),
            Tags = ReadNullableString(reader, "Tags"),
            NumCA = ReadNullableString(reader, "NumCA"),
            ValidadeCA = ReadNullableDateTime(reader, "ValidadeCA"),
            FlagLancamento = reader.GetInt32(reader.GetOrdinal("FlagLancamento")),
            FlagSustentavel = reader.GetInt32(reader.GetOrdinal("FlagSustentavel")),
            CdUnidade = ReadString(reader, "CdUnidade"),
            QtdEmbalagem = ReadNullableInt32(reader, "QtdEmbalagem") ?? 0,
            NmEmbalagem = ReadString(reader, "NmEmbalagem"),
            UnidadeMedida = ReadString(reader, "UnidadeMedida"),
            QtdeCaixaMaster = ReadNullableInt32(reader, "QtdeCaixaMaster") ?? 0,
            CodigoBarras = ReadNullableString(reader, "CodigoBarras"),
            CodDUN = ReadNullableString(reader, "CodDUN"),
            FlagFaltaNoFabricante = reader.GetInt32(reader.GetOrdinal("FlagFaltaNoFabricante")),
            FlagAtivo = ReadNullableInt32(reader, "FlagAtivo") ?? 0,
            FlagCatalogo = reader.GetInt32(reader.GetOrdinal("FlagCatalogo")),
            CdClassificacaoFiscal = ReadString(reader, "CdClassificacaoFiscal"),
            Modelo = ReadNullableString(reader, "Modelo"),
            Normas = ReadNullableString(reader, "Normas"),
            Referencia = ReadNullableString(reader, "Referencia"),
            FSC = ReadNullableString(reader, "FSC"),
            ABNT = ReadNullableString(reader, "ABNT"),
            Anatel = ReadNullableString(reader, "Anatel"),
            Anvisa = ReadNullableString(reader, "Anvisa"),
            Inmetro = ReadNullableString(reader, "Inmetro"),
            FlagDualSourcing = ReadNullableInt32(reader, "FlagDualSourcing") ?? 0,
            Origem = ReadNullableString(reader, "Origem"),
            FlagOutlet = ReadNullableInt32(reader, "FlagOutlet") ?? 0
        };

        await reader.CloseAsync();

        const string sqlProps = """
            SELECT PT.Nome,
                   P.Nome AS Propriedade,
                   IPR.Nome AS Valor
            FROM BR_ItemPropriedade IPR WITH (NOLOCK)
            JOIN BR_Propriedade P WITH (NOLOCK) ON P.PropriedadeID = IPR.PropriedadeID
            JOIN BR_PropriedadeTipo PT (NOLOCK) ON PT.PropriedadeTipoID = P.PropriedadeTipoID
            WHERE IPR.ItemID = @ItemID
            ORDER BY PT.PropriedadeTipoID ASC,
                     P.Ordem ASC
            """;

        await using var cmdProps = new SqlCommand(sqlProps, connection);
        cmdProps.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;

        await using var readerProps = await cmdProps.ExecuteReaderAsync(cancellationToken);
        var props = new List<ProductProperty>();

        while (await readerProps.ReadAsync(cancellationToken))
        {
            props.Add(new ProductProperty
            {
                Tipo = ReadString(readerProps, "Nome"),
                Nome = ReadString(readerProps, "Propriedade"),
                Valor = ReadString(readerProps, "Valor")
            });
        }

        detail.Propriedades = props;
        return detail;
    }

    public async Task<IReadOnlyList<ProductStockEstablishment>> GetProductStockAsync(int itemId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DECLARE @TblEstoqueVirtual TABLE(
                EstabelecimentoID INT INDEX IDX01,
                QtEstoqueVirtual INT
            )
            INSERT INTO @TblEstoqueVirtual (
                EstabelecimentoID,
                QtEstoqueVirtual
            ) SELECT 9,
                     VIRT.QtdTotal
            FROM BrSupply_Aux.dbo.BR_PrecoEstoque_DispSAP VIRT WITH (NOLOCK)
            WHERE VIRT.ItemID = @ItemID
              AND VIRT.EstabelecimentoID = 14

            DECLARE @TblEstoqueWMS TABLE (    
                EstabelecimentoID INT INDEX IDX01,
                QtEstoqueWMS INT,
                QtProcessamentoWMS INT
            )
            INSERT INTO @TblEstoqueWMS (
                EstabelecimentoID,
                QtEstoqueWMS,
                QtProcessamentoWMS
            ) SELECT E.EstabelecimentoID,
                     CONVERT(INT, W.QtEstoque) + CONVERT(INT, W.QtRecebimento),
                     CONVERT(INT, W.QtProcessamento)
              FROM BRSupply_Aux..BR_PrecoEstoqueWMS W WITH (NOLOCK)
              JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.CdEstabelecimento = W.CdEstabelecimento
              WHERE W.CdItem = (SELECT CdItem FROM BR_Item WITH (NOLOCK) WHERE ItemID = @ItemID)
                AND W.CdEstabelecimento = E.CdEstabelecimento

            SELECT E.NmEstabelecimento,
                   E.NmCurto,
                   E.EstabelecimentoID,
                   E.CdEstabelecimento,
                   CONVERT(INT,ISNULL(DS.QtdTotal,0)) AS QtdContabilSAP,
                   IIF(P.EstabelecimentoID = 9, VIRT.QtEstoqueVirtual, 0) AS QtEstoqueVirtualSP,
                   CONVERT(INT,ISNULL(DS.QtdReservado,0)) AS QtdRemessaSAP,
                   ISNULL(DS.QtdEmProcessamento,0) AS QtdProcessamentoSAP,
                   CONVERT(INT,ISNULL(DS.QtdTotal,0) - ISNULL(DS.QtdReservado,0) - ISNULL(DS.QtdEmProcessamento,0)) AS QtdDisponivelSAP,
                   ISNULL(P.QtAlocadaSemOV, 0) AS QtAlocadaSemOVSAP,
                   CONVERT(INT,ISNULL(P.QtAlocadaPedido,0) - ISNULL(P.QtAlocadaSemOV, 0)) AS QtAlocadaComOVSAP,
                   CONVERT(INT,ISNULL(P.QtAlocadaPedido,0)) AS QtAlocadaSIC,
                   ISNULL(P.QtNaoDebitaEstoque,0) AS QtNaoDebitaEstoqueSIC,
                   CONVERT(INT,(ISNULL(P.QtDispEstoque,0) - ISNULL(P.QtAlocadaSemOV,0))) AS QtDisponivelSIC,
                   ISNULL(DS.QtdEstoque,0) AS QtdEstoqueSAP,
                   CONVERT(INT,ISNULL(P.QtDispEstoque,0)) AS QtEstoque,
                   CONVERT(INT,ISNULL(P.QtAlocada,0)) AS QtReservadaSIC,
                   ISNULL(WMS.QtEstoqueWMS, 0) AS QtEstoqueWMS,
                   ISNULL(WMS.QtProcessamentoWMS, 0) AS QtProcessamentoWMS,  
                   P.VlrCustoAquisicao,
                   P.VlrCustoMedio,
                   P.FollowComprasNegociacao,
                   P.DtFollowComprasNegociacao,
                   CASE WHEN P.DtFollowCompras >= GETDATE() 
                        THEN ISNULL(P.DsFollowCompras,'')
                        ELSE ''
                   END AS DsFollowCompras,
                   P.DtFollowCompras,
                   CASE ISNULL(P.Curva, '')
                        WHEN '' THEN '-'
                        ELSE P.Curva
                   END AS Curva,
                   IIF(I.FlagFaltaNoFabricante = 0, (
                        CASE WHEN ISNULL(P.FlagOutlet, 0) = 1 THEN 'Y'
                             ELSE CASE
                                    WHEN ISNULL(P.FlagSobDemanda, 0) = 1 THEN 'Z'
                                    ELSE 'X'
                                  END
                        END),
                   'F') AS Criticidade,
                   ISNULL(P.FlagOutlet, 0) AS FlagOutlet,
                   ISNULL(P.FlagSobDemanda, 0) AS FlagSobDemanda,
                   ISNULL(P.FlagOcultoEstoqueZero, 0) AS FlagOcultoEstoqueZero,
                   ISNULL((
                          SELECT MIN(FTI.DiasLeadTime)
                        FROM Integracao_Clientes.dbo.SRM_FornecedorTblPreco FT WITH (NOLOCK)
                        JOIN Integracao_Clientes.dbo.SRM_FornecedorTblPrecoItem FTI WITH (NOLOCK) ON FTI.FornecedorTblPrecoID = FT.FornecedorTblPrecoID
                        WHERE FT.EstabelecimentoID = P.EstabelecimentoID
                          AND FTI.ItemID = P.ItemID
                          AND FT.DtInicio < CONVERT(DATE,GETDATE())
                          AND FT.DtTermino > CONVERT(DATE,GETDATE()) ),0) AS MinLeadTime,
                   ISNULL((
                          SELECT MAX(FTI.DiasLeadTime)
                        FROM Integracao_Clientes.dbo.SRM_FornecedorTblPreco FT WITH (NOLOCK)
                        JOIN Integracao_Clientes.dbo.SRM_FornecedorTblPrecoItem FTI WITH (NOLOCK) ON FTI.FornecedorTblPrecoID = FT.FornecedorTblPrecoID
                        WHERE FT.EstabelecimentoID = P.EstabelecimentoID
                          AND FTI.ItemID = P.ItemID
                          AND FT.DtInicio < CONVERT(DATE,GETDATE())
                          AND FT.DtTermino > CONVERT(DATE,GETDATE()) ),0) AS MaxLeadTime,
                   ISNULL((
                        SELECT TOP 1 FORMAT(FT.DtTermino, 'dd/MM/yyyy') + '|' + FT.NmTblPreco + '|' + F.RazaoSocial
                        FROM Integracao_Clientes.dbo.SRM_FornecedorTblPreco FT (NOLOCK)
                        INNER JOIN Integracao_Clientes.dbo.SRM_FornecedorTblPrecoItem FTI (NOLOCK) ON FTI.FornecedorTblPrecoID = FT.FornecedorTblPrecoID
                        INNER JOIN Integracao_Clientes.dbo.SRM_Fornecedor F (NOLOCK) ON F.FornecedorID = FT.FornecedorID
                        WHERE FT.EstabelecimentoID = P.EstabelecimentoID
                            AND FTI.ItemID = P.ItemID
                            AND FT.DtTermino >= GETDATE()
                        ORDER BY FT.DtTermino DESC
                    ), '') AS DetalhesCustoAquisicao,
                   CO.NmUsuario AS NmComprador,
                   CO.Email AS EmailComprador,
                   CO.Foto AS FotoComprador,
                   GE.NmUsuario AS NmGestor,
                   GE.Email AS EmailGestor,
                   GE.Foto AS FotoGestor,
                   COI.NmUsuario AS NmCompradorInternacional,
                   COI.Email AS EmailCompradorInternacional,
                   COI.Foto AS FotoCompradorInternacional
            FROM BR_PrecoEstoque P WITH (NOLOCK)
            JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = P.EstabelecimentoID
            JOIN BR_Item I WITH (NOLOCK) ON I.ItemID = P.ItemID
            LEFT JOIN BrSupply_Aux.dbo.BR_PrecoEstoque_DispSAP DS WITH (NOLOCK) ON DS.ItemID = P.ItemID AND DS.EstabelecimentoID = P.EstabelecimentoID
            LEFT JOIN @TblEstoqueVirtual VIRT ON VIRT.EstabelecimentoID = P.EstabelecimentoID
            LEFT JOIN @TblEstoqueWMS WMS ON WMS.EstabelecimentoID = P.EstabelecimentoID
            LEFT JOIN BR_Usuario GE WITH (NOLOCK) ON GE.UsuarioID = P.UsuarioGestorID
            LEFT JOIN BR_Usuario CO WITH (NOLOCK) ON CO.UsuarioID = P.UsuarioCompradorID
            LEFT JOIN BR_Usuario COI WITH (NOLOCK) ON COI.UsuarioID = P.CompradorInternacionalID
            WHERE E.FlagAtivo = 1
              AND ISNULL(E.NmCurto,'') <> ''
              AND P.ItemID = @ItemID
            ORDER BY E.CdEstabelecimento DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ProductStockEstablishment>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductStockEstablishment
            {
                NmEstabelecimento = ReadString(reader, "NmEstabelecimento"),
                NmCurto = ReadString(reader, "NmCurto"),
                EstabelecimentoID = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                CdEstabelecimento = ReadString(reader, "CdEstabelecimento"),
                QtdContabilSAP = ReadNullableInt32(reader, "QtdContabilSAP") ?? 0,
                QtEstoqueVirtualSP = ReadNullableInt32(reader, "QtEstoqueVirtualSP") ?? 0,
                QtdRemessaSAP = ReadNullableInt32(reader, "QtdRemessaSAP") ?? 0,
                QtdProcessamentoSAP = ReadNullableInt32(reader, "QtdProcessamentoSAP") ?? 0,
                QtdDisponivelSAP = ReadNullableInt32(reader, "QtdDisponivelSAP") ?? 0,
                QtAlocadaSemOVSAP = ReadNullableInt32(reader, "QtAlocadaSemOVSAP") ?? 0,
                QtAlocadaComOVSAP = ReadNullableInt32(reader, "QtAlocadaComOVSAP") ?? 0,
                QtAlocadaSIC = ReadNullableInt32(reader, "QtAlocadaSIC") ?? 0,
                QtNaoDebitaEstoqueSIC = ReadNullableInt32(reader, "QtNaoDebitaEstoqueSIC") ?? 0,
                QtDisponivelSIC = ReadNullableInt32(reader, "QtDisponivelSIC") ?? 0,
                QtdEstoqueSAP = ReadNullableInt32(reader, "QtdEstoqueSAP") ?? 0,
                QtEstoque = ReadNullableInt32(reader, "QtEstoque") ?? 0,
                QtReservadaSIC = ReadNullableInt32(reader, "QtReservadaSIC") ?? 0,
                QtEstoqueWMS = ReadNullableInt32(reader, "QtEstoqueWMS") ?? 0,
                QtProcessamentoWMS = ReadNullableInt32(reader, "QtProcessamentoWMS") ?? 0,
                VlrCustoAquisicao = ReadNullableDecimal(reader, "VlrCustoAquisicao"),
                VlrCustoMedio = ReadNullableDecimal(reader, "VlrCustoMedio"),
                FollowComprasNegociacao = ReadNullableString(reader, "FollowComprasNegociacao"),
                DtFollowComprasNegociacao = ReadNullableDateTime(reader, "DtFollowComprasNegociacao"),
                DsFollowCompras = ReadNullableString(reader, "DsFollowCompras"),
                DtFollowCompras = ReadNullableDateTime(reader, "DtFollowCompras"),
                Curva = ReadString(reader, "Curva"),
                Criticidade = ReadString(reader, "Criticidade"),
                FlagOutlet = ReadNullableInt32(reader, "FlagOutlet") ?? 0,
                FlagSobDemanda = ReadNullableInt32(reader, "FlagSobDemanda") ?? 0,
                FlagOcultoEstoqueZero = ReadNullableInt32(reader, "FlagOcultoEstoqueZero") ?? 0,
                MinLeadTime = ReadNullableInt32(reader, "MinLeadTime") ?? 0,
                MaxLeadTime = ReadNullableInt32(reader, "MaxLeadTime") ?? 0,
                DetalhesCustoAquisicao = ReadNullableString(reader, "DetalhesCustoAquisicao"),
                NmComprador = ReadNullableString(reader, "NmComprador"),
                EmailComprador = ReadNullableString(reader, "EmailComprador"),
                FotoComprador = ReadNullableString(reader, "FotoComprador"),
                NmGestor = ReadNullableString(reader, "NmGestor"),
                EmailGestor = ReadNullableString(reader, "EmailGestor"),
                FotoGestor = ReadNullableString(reader, "FotoGestor"),
                NmCompradorInternacional = ReadNullableString(reader, "NmCompradorInternacional"),
                EmailCompradorInternacional = ReadNullableString(reader, "EmailCompradorInternacional"),
                FotoCompradorInternacional = ReadNullableString(reader, "FotoCompradorInternacional")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProductStockAllocation>> GetProductStockAllocationsAsync(int itemId, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT C.CotacaoID AS Pedido,
                   C.DtCotacao AS DtPedido,
                   C.DtProgLiberacao,
                   L.NmCliente,
                   S.DsStatusCotacao,
                   E.CdEstabelecimento,
                   CONVERT(INT, I.QtItem) AS QtSolicitada,
                   (SELECT COUNT(*)
                    FROM BR_CotacaoItem W WITH (NOLOCK)
                    WHERE W.CotacaoID = C.CotacaoID
                      AND ISNULL(W.ItemID,0) > 0
                      AND ISNULL(W.FlagAlocaPedido,0) = 0) AS QtRupturas,
                   V.NmCanalVenda AS NmCanalVenda,
                   ISNULL((SELECT TOP 1 P.OrdemVenda
                           FROM Integracao_Clientes..BR_SAP_Pedidos P WITH (NOLOCK)
                           WHERE P.CotacaoID = C.CotacaoID
                             AND ISNULL(P.RemessaSAP,'') <> ''
                             AND ISNULL(P.OrdemVenda,'') <> ''), 'Sem OV') AS OrdemVendaSAP
            FROM BR_Cotacao C WITH (NOLOCK)
            JOIN BR_CotacaoItem I WITH (NOLOCK) ON I.CotacaoID = C.CotacaoID
            JOIN BR_Cliente L WITH (NOLOCK) ON L.ClienteID = C.ClienteID
            JOIN BR_StatusCotacao S WITH (NOLOCK) ON S.StatusCotacao = C.StatusCotacao
            JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = C.EstabelecimentoID
            JOIN BR_CanalVenda V WITH (NOLOCK) ON V.CanalVendaID = C.CanalVendaID
            WHERE I.ItemID = @ItemID
              AND C.EstabelecimentoID = @EstabelecimentoID
              AND I.FlagAlocaPedido = 1
              AND V.FlagNaoDebitaEstoque = 0
            ORDER BY C.CotacaoID
            """;

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;
        cmd.Parameters.Add("@EstabelecimentoID", SqlDbType.Int).Value = estabelecimentoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ProductStockAllocation>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductStockAllocation
            {
                Pedido = reader.GetInt32(reader.GetOrdinal("Pedido")),
                DtPedido = reader.GetDateTime(reader.GetOrdinal("DtPedido")),
                DtProgLiberacao = ReadNullableDateTime(reader, "DtProgLiberacao"),
                NmCliente = ReadString(reader, "NmCliente"),
                DsStatusCotacao = ReadString(reader, "DsStatusCotacao"),
                CdEstabelecimento = ReadString(reader, "CdEstabelecimento"),
                QtSolicitada = ReadNullableInt32(reader, "QtSolicitada") ?? 0,
                QtRupturas = ReadNullableInt32(reader, "QtRupturas") ?? 0,
                NmCanalVenda = ReadString(reader, "NmCanalVenda"),
                OrdemVendaSAP = ReadString(reader, "OrdemVendaSAP")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProductPurchaseOrder>> GetProductPurchaseOrdersAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT CONVERT(INT, I.QtItemCompra) AS Quantidade,
                   I.DtPrevEntrega AS DtPrevisao,
                   I.NrOrdemCompra AS OrdemCompra,
                   I.NrOrdemItem AS XPed,
                   E.NmEstabelecimento,
                   E.CdEstabelecimento,
                   F.RazaoSocial
            FROM BR_ItemEntrega I WITH (NOLOCK)
            JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = I.EstabelecimentoID
            JOIN Integracao_Clientes..SRM_Fornecedor F WITH (NOLOCK) ON F.FornecedorID = I.FornecedorID
            WHERE I.ItemID = @ItemID
            ORDER BY I.DtPrevEntrega
            """;

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ProductPurchaseOrder>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductPurchaseOrder
            {
                Quantidade = ReadNullableInt32(reader, "Quantidade") ?? 0,
                DtPrevisao = ReadNullableDateTime(reader, "DtPrevisao"),
                OrdemCompra = ReadString(reader, "OrdemCompra"),
                XPed = ReadString(reader, "XPed"),
                NmEstabelecimento = ReadString(reader, "NmEstabelecimento"),
                CdEstabelecimento = ReadString(reader, "CdEstabelecimento"),
                RazaoSocial = ReadString(reader, "RazaoSocial")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProductSimilar>> GetProductSimilarsAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT I.ItemID,
                   I.CdItem,
                   I.NmItem,
                   S.DataHora AS DataHoraCadastro,
                   C.CdClassificacaoFiscal AS NCM
            FROM BR_ItemSimilarTroca S WITH (NOLOCK)
            JOIN BR_Item I WITH (NOLOCK) ON I.ItemID = S.ItemSimilarID
            JOIN BR_ClassificacaoFiscal C (NOLOCK) ON C.ClassificacaoFiscalID = I.ClassificacaoFiscalID
            WHERE S.ItemID = @ItemID
              AND S.DataHora >= DATEADD(MONTH, -24, GETDATE())
            ORDER BY I.NmItem
            """;

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ProductSimilar>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductSimilar
            {
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                CdItem = ReadString(reader, "CdItem"),
                NmItem = ReadString(reader, "NmItem"),
                DataHoraCadastro = reader.GetDateTime(reader.GetOrdinal("DataHoraCadastro")),
                NCM = ReadString(reader, "NCM")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProductSimilarStock>> GetProductSimilarStockAsync(int itemSimilarId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT E.CdEstabelecimento,
                   E.NmEstabelecimento AS NmEstabelecimento,
                   P.Curva,
                   IIF(I.FlagFaltaNoFabricante = 0, (
                        CASE WHEN ISNULL(P.FlagOutlet, 0) = 1 THEN 'Y'
                             ELSE CASE
                                    WHEN ISNULL(P.FlagSobDemanda, 0) = 1 THEN 'Z'
                                    ELSE 'X'
                                  END
                        END),
                   'F') AS Criticidade,
                   IIF(ISNULL(I.FlagAtivo, 0) = 0, 'Inativo',
                       IIF(I.FlagFaltaNoFabricante = 0, (
                            CASE WHEN ISNULL(P.FlagOutlet, 0) = 1 THEN 'Outlet'
                                 ELSE CASE
                                        WHEN ISNULL(P.FlagSobDemanda, 0) = 1 THEN 'Sob Demanda'
                                        ELSE ''
                                      END
                            END),
                       'Falta no Fabricante')) AS Situacao,
                   CONVERT(INT, (P.QtDispEstoque - P.QtAlocadaSemOV)) AS QtDisponivel
            FROM BR_PrecoEstoque P WITH (NOLOCK)
            JOIN BR_Item I WITH (NOLOCK) ON I.ItemID = P.ItemID
            JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = P.EstabelecimentoID
            WHERE P.ItemID = @ItemSimilarID
              AND ISNULL(E.NmCurto,'') <> ''
            """;

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ItemSimilarID", SqlDbType.Int).Value = itemSimilarId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ProductSimilarStock>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductSimilarStock
            {
                CdEstabelecimento = ReadString(reader, "CdEstabelecimento"),
                NmEstabelecimento = ReadString(reader, "NmEstabelecimento"),
                Curva = ReadString(reader, "Curva"),
                Criticidade = ReadString(reader, "Criticidade"),
                Situacao = ReadString(reader, "Situacao"),
                QtDisponivel = ReadNullableInt32(reader, "QtDisponivel") ?? 0
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<RelatedProduct>> GetRelatedProductsAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT DISTINCT A.ItemID, A.CdItem, A.NmItem
            FROM (
                SELECT R.ItemID1 AS ItemID, I.CdItem, I.NmItem
                FROM BR_ItemRelacionado R (NOLOCK)
                JOIN BR_Item I (NOLOCK) ON I.ItemID = R.ItemID1 AND I.FlagAtivo = 1
                AND ItemID2 = @ItemID
                UNION
                SELECT R.ItemID2 AS ItemID, I.CdItem, I.NmItem
                FROM BR_ItemRelacionado R (NOLOCK)
                JOIN BR_Item I (NOLOCK) ON I.ItemID = R.ItemID2 AND I.FlagAtivo = 1
                AND ItemID1 = @ItemID
            ) A
            """;

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ItemID", SqlDbType.Int).Value = itemId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<RelatedProduct>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new RelatedProduct
            {
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                CdItem = ReadString(reader, "CdItem"),
                NmItem = ReadString(reader, "NmItem")
            });
        }

        return items;
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static string? ReadNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? ReadNullableInt32(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static DateTime? ReadNullableDateTime(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static decimal ReadDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
    }

    private static decimal? ReadNullableDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }
}
