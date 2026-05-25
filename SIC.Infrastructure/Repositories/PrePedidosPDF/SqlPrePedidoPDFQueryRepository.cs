using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.PrePedidosPDF;
using SIC.Domain.Entities.PrePedidosPDF;
using System.Data;
using System.Text;

namespace SIC.Infrastructure.Repositories.PrePedidosPDF;

/// <summary>
/// Implementação SQL das operações de leitura do pré-pedido.
/// </summary>
public sealed class SqlPrePedidoPDFQueryRepository(IConfiguration configuration) : IPrePedidoPDFQueryRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    private const string DetalheSql = """
        SELECT pdfPP.PDFPrePedidoID,
               pdfAPP.Arquivo,
               REPLACE(pdfAPP.Arquivo, '.PDF', '') AS ArquivoFormat,
               pdfPP.DataOrdemCompra AS OrdemCompraDataHoraFormat,
               PPC.ClienteUsuarioID AS CadastroUsuarioID,
               U.NmUsuario AS CadastroNmUsuario,
               pdfPP.StatusPrePedidoID,
               pdfPP.CotacaoID,
               pdfPP.OrdemCompra,
               pdfPP.CNPJ,
               pdfPP.ClienteLocalEntregaID,
               pdfPP.ClienteEnderecoID,
               (C.CdExtCliente + ' - ' + C.NmCliente) AS Cliente,
               E.NmEstabelecimento AS Estabelecimento,
               E.EstabelecimentoID,
               Ed.Logradouro AS Endereco,
               (L.CdControle + ' - ' + L.NmLocalEntrega) AS NmLocalEntrega,
               endCp.NmCondPagto AS CondPagto,
               IIF(ISNULL(locV.CanalVendaID, 0) > 0, locV.NmCanalVenda, V.NmCanalVenda) AS CanalVenda,
               C.tipoOVSAP AS TipoOVSAP,
               T.NmTblPreco AS TabelaPreco,
               PPS.Descricao AS StatusDescricao,
               C.CdExtCliente,
               C.ClienteID,
               pdfPP.TblPrecoID,
               C.LogoCliente,
               C.NmCliente,
               C.VlrMinimoBloqueioPedido,
               pdfPP.ObsNota,
               pdfPP.ObsComprador,
               pdfPP.ClienteCategoriaPedidoID,
               CCP.NmCategoria AS NmCategoriaPedido
        FROM Integracao_Clientes.dbo.PDF_PrePedido pdfPP WITH (NOLOCK)
        INNER JOIN Integracao_Clientes.dbo.PDF_ArquivoPrePedido pdfAPP WITH (NOLOCK) ON pdfAPP.PDFArquivoPrePedidoID = pdfPP.ArquivoPrePedidoId
        INNER JOIN Integracao_Clientes.dbo.PPedido_ProcessadorPedidoConfiguracao PPC WITH (NOLOCK) ON PPC.ClienteID = pdfPP.ClienteID
        LEFT JOIN Integracao_Clientes.dbo.PPedido_StatusPrePedido PPS WITH (NOLOCK) ON PPS.StatusPrePedidoID = pdfPP.StatusPrePedidoID
        LEFT JOIN BrSupply.dbo.BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = PPC.ClienteUsuarioID
        LEFT JOIN BrSupply.dbo.BR_Cliente C WITH (NOLOCK) ON C.ClienteID = pdfPP.ClienteID
        LEFT JOIN BrSupply.dbo.BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = C.EstabelecimentoID
        LEFT JOIN BrSupply.dbo.BR_ClienteLocalEntrega L WITH (NOLOCK) ON L.ClienteLocalEntregaID = pdfPP.ClienteLocalEntregaID
        LEFT JOIN BrSupply.dbo.BR_ClienteEndereco Ed WITH (NOLOCK) ON Ed.ClienteEnderecoID = pdfPP.ClienteEnderecoID
        LEFT JOIN BrSupply.dbo.BR_CondPagto endCp WITH (NOLOCK) ON endCp.CondPagtoID = Ed.CondPagtoID
        LEFT JOIN BrSupply.dbo.BR_CanalVenda locV WITH (NOLOCK) ON locV.CanalVendaID = L.CanalVendaID
        LEFT JOIN BrSupply.dbo.BR_CanalVenda V WITH (NOLOCK) ON V.CanalVendaID = C.CanalVendaID
        LEFT JOIN BrSupply.dbo.BR_TblPreco T WITH (NOLOCK) ON T.TblPrecoID = Ed.TblPrecoID
        LEFT JOIN BrSupply.dbo.BR_ClienteCategoriaPedido CCP WITH (NOLOCK) ON CCP.ClienteCategoriaPedidoID = pdfPP.ClienteCategoriaPedidoID
        WHERE pdfPP.PDFPrePedidoID = @PDFPrePedidoID
        ORDER BY pdfPP.PDFPrePedidoID DESC
        """;

    private const string ItensSql = """
        SELECT pdfPI.PDFPrePedidoItemID,
               pdfPI.PDFPrePedidoID,
               pdfPI.Sequencia AS PDFSeqItem,
               CONVERT(INT, ROUND(pdfPI.Quantidade, 0)) AS PDFQtde,
               pdfPI.ItemID AS ItemInternoID,
               pdfPI.CdItemCliente + ' - ' + pdfPI.Descricao AS ItemCliente,
               pdfPI.Descricao,
               I.CdItem AS ItemID,
               (I.CdItem + ' - ' + I.NmItem) AS ItemBrSupply,
               I.SegmentoID,
               I.FamiliaID,
               FORMAT(TI.VlrUnit, 'C', 'pt-br') AS VlrTblPrecoFormat,
               REPLACE(pdfPI.ValorUnitario, '.', ',') AS PDFVlrUnit,
               FORMAT((pdfPI.Quantidade * pdfPI.ValorUnitario), 'C', 'pt-br') AS VlrTotal,
               FORMAT(SUM(pdfPI.Quantidade * pdfPI.ValorUnitario) OVER (PARTITION BY pdfPI.PDFPrePedidoID), 'C', 'pt-br') AS VlrTotalPedido
        FROM Integracao_Clientes.dbo.PDF_PrePedidoItem pdfPI WITH (NOLOCK)
        LEFT JOIN BrSupply.dbo.BR_Item I WITH (NOLOCK) ON I.ItemID = pdfPI.ItemID
        LEFT JOIN Integracao_Clientes.dbo.PDF_PrePedido pdfPP WITH (NOLOCK) ON pdfPP.PDFPrePedidoID = pdfPI.PDFPrePedidoID
        LEFT JOIN BrSupply.dbo.BR_Cliente C WITH (NOLOCK) ON C.ClienteID = pdfPP.ClienteID
        LEFT JOIN BrSupply.dbo.BR_ClienteEndereco E WITH (NOLOCK) ON E.ClienteEnderecoID = pdfPP.ClienteEnderecoID
        LEFT JOIN BrSupply.dbo.BR_TblPreco T WITH (NOLOCK) ON T.TblPrecoID = E.TblPrecoID
        LEFT JOIN BrSupply.dbo.BR_TblPrecoVig V WITH (NOLOCK) ON V.TblPrecoID = T.TblPrecoID
        LEFT JOIN BrSupply.dbo.BR_TblPrecoItem TI WITH (NOLOCK) ON TI.TblPrecoVigID = V.TblPrecoVigID AND TI.ItemID = I.ItemID
        LEFT JOIN BrSupply.dbo.BR_PrecoEstoque PE WITH (NOLOCK) ON PE.EstabelecimentoID = C.EstabelecimentoID AND PE.ItemID = I.ItemID
        WHERE pdfPI.PDFPrePedidoID = @PDFPrePedidoID
        ORDER BY CAST(pdfPI.Sequencia AS INT) ASC
        """;

    private const string LogsSql = """
        SELECT Mensagem,
               CONVERT(VARCHAR(10), CriadoEm, 103) + ' ' + CONVERT(VARCHAR(8), CriadoEm, 108) AS CriadoEmFormatado,
               ISNULL(Tipo, '') AS Tipo
        FROM Integracao_Clientes.dbo.PDF_PrePedidoLog
        WHERE PDFPrePedidoID = @PDFPrePedidoID
        ORDER BY CriadoEm DESC
        """;

    private const string EnderecosSql = """
        SELECT ClienteEnderecoID,
               Logradouro
        FROM BrSupply.dbo.BR_ClienteEndereco
        WHERE ClienteID = @ClienteID
          AND FlagAtivo = 1
        """;

    private const string LocaisEntregaSql = """
        SELECT ClienteLocalEntregaID,
               NmLocalEntrega,
               CdControle
        FROM BrSupply.dbo.BR_ClienteLocalEntrega
        WHERE ClienteEnderecoID = @ClienteEnderecoID
          AND FlagAtivo = 1
        """;

    private const string CnpjsSql = """
        SELECT ClienteEnderecoID,
               CPFCNPJ
        FROM BrSupply.dbo.BR_ClienteEndereco
        WHERE ClienteID = @ClienteID
          AND FlagAtivo = 1
        """;

    private const string LogsErroSql = """
        SELECT COUNT(*) AS Registros
        FROM Integracao_Clientes.dbo.PDF_PrePedidoLog
        WHERE PDFPrePedidoID = @PDFPrePedidoID
          AND LOWER(ISNULL(Tipo, '')) IN ('erro', 'error')
        """;

    private const string TrocaItensSql = """
        SELECT I.CdItem,
               I.NmItem,
               I.ItemID,
               CONVERT(DECIMAL(10,2),(
                    SELECT TPI.VlrUnit
                    FROM BrSupply.dbo.BR_TblPrecoItem TPI WITH (NOLOCK)
                    JOIN BrSupply.dbo.BR_TblPrecoVig TPV WITH (NOLOCK) ON TPV.TblPrecoVigID = TPI.TblPrecoVigID
                    WHERE TPI.ItemID = I.ItemID
                      AND TPV.TblPrecoID = @TblPrecoID
               )) AS VlrTabelaPreco
        FROM BrSupply.dbo.BR_Item I WITH (NOLOCK)
        LEFT JOIN BrSupply.dbo.BR_PrecoEstoque E WITH (NOLOCK) ON E.ItemID = I.ItemID AND E.EstabelecimentoID = @EstabelecimentoID
        WHERE ISNULL(I.FlagAtivo,0) = 1
          AND I.SegmentoID = @SegmentoID
          AND I.FamiliaID = @FamiliaID
          AND I.ItemID <> @ItemID
          AND EXISTS (
                SELECT TPI.VlrUnit
                FROM BrSupply.dbo.BR_TblPrecoItem TPI WITH (NOLOCK)
                JOIN BrSupply.dbo.BR_TblPrecoVig TPV WITH (NOLOCK) ON TPV.TblPrecoVigID = TPI.TblPrecoVigID
                WHERE TPI.ItemID = I.ItemID
                  AND TPV.TblPrecoID = @TblPrecoID)
        ORDER BY I.NmItem
        """;

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
            , FORMAT(ISNULL(PE.VlrCustoAquisicao,0),'N','pt-br') AS VlrCustoAquisicao
            , FORMAT(ISNULL(PE.VlrCustoMedio,0),'N','pt-br') AS VlrCustoMedio
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
            , FORMAT((
                ISNULL((
                    T.VlrTabela
                ), 0)
            ), 'N', 'pt-br') AS TabelaPreco
            , DP.ItemCli1 AS ItemDePara
        FROM @Tbl T
        INNER JOIN BrSupply.dbo.BR_Item I (NOLOCK) ON I.ItemID = T.ItemID
        INNER JOIN BrSupply.dbo.BR_Segmento S (NOLOCK) ON S.SegmentoID = I.SegmentoID
        INNER JOIN BrSupply.dbo.BR_Familia F (NOLOCK) ON F.FamiliaID = I.FamiliaID
        INNER JOIN BrSupply.dbo.BR_SubFamilia SF (NOLOCK) ON SF.SubFamiliaID = I.SubFamiliaID
        INNER JOIN BrSupply.dbo.BR_PrecoEstoque PE (NOLOCK) ON PE.ItemID = I.ItemID
        LEFT JOIN BrSupply.dbo.BR_ProdutoMarca M (NOLOCK) ON M.ProdutoMarcaID = I.ProdutoMarcaID
        INNER JOIN Integracao_Clientes.dbo.BR_Itens_DePara DP ON DP.ItemBR = I.CdItem AND DP.ClienteID = @ClienteID
        WHERE 1=1
            AND PE.EstabelecimentoID = @EstabelecimentoID
            AND T.VlrTabela <> 0
        ORDER BY
            T.Probabilidade DESC
            , ISNULL(S.FlagConsultaProduto,99) ASC
            , I.FlagAtivo DESC
            , ISNULL(M.FlagTipoMarca,'zzz') ASC
            , ISNULL(PE.FlagOutlet, 0) ASC
        """;

    public async Task<IReadOnlyList<PrePedidoPDFListItem>> GetListAsync(
        int? status,
        string? cdExtCliente,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            SELECT PP.PDFPrePedidoID,
                   AP.ClienteID,
                   AP.OrdemCompra,
                   PP.StatusPrePedidoID,
                   PP.CotacaoID,
                   CONCAT(C.CdExtCliente, ' - ', C.NmCliente) AS NmCliente,
                   PP.CNPJ,
                   PPS.Descricao AS StatusDescricao,
                   CONVERT(VARCHAR(10), PP.CriadoEm, 103) + ' ' + CONVERT(VARCHAR(8), PP.CriadoEm, 108) AS CriadoEm
            FROM Integracao_Clientes.dbo.PDF_PrePedido PP WITH (NOLOCK)
            JOIN Integracao_Clientes.dbo.PDF_ArquivoPrePedido AP WITH (NOLOCK) ON PP.ArquivoPrePedidoID = AP.PDFArquivoPrePedidoID
            LEFT JOIN Integracao_Clientes.dbo.PPedido_StatusPrePedido PPS WITH (NOLOCK) ON PPS.StatusPrePedidoID = PP.StatusPrePedidoID
            JOIN BrSupply.dbo.BR_Cliente C WITH (NOLOCK) ON C.ClienteID = AP.ClienteID
            WHERE 1 = 1
            """);

        if (status.HasValue && status.Value != 0)
            sb.AppendLine("AND PP.StatusPrePedidoID = @Status");

        if (!string.IsNullOrWhiteSpace(cdExtCliente))
            sb.AppendLine("AND C.CdExtCliente = @CdExtCliente");

        if (dataInicial.HasValue && dataFinal.HasValue)
            sb.AppendLine("AND PP.CriadoEm BETWEEN @DataInicial AND @DataFinal");
        else if (dataInicial.HasValue)
            sb.AppendLine("AND PP.CriadoEm >= @DataInicial");
        else if (dataFinal.HasValue)
            sb.AppendLine("AND PP.CriadoEm <= @DataFinal");

        sb.AppendLine("ORDER BY PP.PDFPrePedidoID DESC");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sb.ToString(), connection);

        if (status.HasValue && status.Value != 0)
            cmd.Parameters.AddWithValue("@Status", status.Value);

        if (!string.IsNullOrWhiteSpace(cdExtCliente))
            cmd.Parameters.AddWithValue("@CdExtCliente", cdExtCliente);

        if (dataInicial.HasValue)
            cmd.Parameters.AddWithValue("@DataInicial", dataInicial.Value.Date);

        if (dataFinal.HasValue)
            cmd.Parameters.AddWithValue("@DataFinal", dataFinal.Value.Date.AddDays(1).AddSeconds(-1));

        var items = new List<PrePedidoPDFListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFListItem
            {
                PDFPrePedidoPDFID = reader.GetInt32(reader.GetOrdinal("PDFPrePedidoID")),
                ClienteID = reader.GetInt32(reader.GetOrdinal("ClienteID")),
                OrdemCompra = reader.IsDBNull(reader.GetOrdinal("OrdemCompra")) ? string.Empty : reader.GetString(reader.GetOrdinal("OrdemCompra")),
                StatusPrePedidoPDFID = reader.IsDBNull(reader.GetOrdinal("StatusPrePedidoID")) ? 0 : reader.GetInt32(reader.GetOrdinal("StatusPrePedidoID")),
                CotacaoID = reader.IsDBNull(reader.GetOrdinal("CotacaoID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CotacaoID")),
                NmCliente = reader.IsDBNull(reader.GetOrdinal("NmCliente")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmCliente")),
                CNPJ = reader.IsDBNull(reader.GetOrdinal("CNPJ")) ? string.Empty : reader.GetString(reader.GetOrdinal("CNPJ")),
                StatusDescricao = reader.IsDBNull(reader.GetOrdinal("StatusDescricao")) ? string.Empty : reader.GetString(reader.GetOrdinal("StatusDescricao")),
                CriadoEm = reader.IsDBNull(reader.GetOrdinal("CriadoEm")) ? string.Empty : reader.GetString(reader.GetOrdinal("CriadoEm")),
            });
        }

        return items;
    }

    public async Task<PrePedidoPDFDetalhe?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var detalhe = await GetDetalheAsync(connection, id, cancellationToken);

        if (detalhe is null)
            return null;

        detalhe.Itens = await GetItensAsync(connection, id, cancellationToken);
        detalhe.Logs = await GetLogsAsync(connection, id, cancellationToken);
        detalhe.Enderecos = await GetEnderecosAsync(connection, detalhe.ClienteID, cancellationToken);
        detalhe.LocaisEntrega = detalhe.ClienteEnderecoID > 0
            ? await GetLocaisEntregaAsync(connection, detalhe.ClienteEnderecoID, cancellationToken)
            : [];
        detalhe.Cnpjs = await GetCnpjsAsync(connection, detalhe.ClienteID, cancellationToken);
        detalhe.QtdLogsErro = await GetQtdLogsErroAsync(connection, id, cancellationToken);

        return detalhe;
    }

    public async Task<IReadOnlyList<PrePedidoPDFLocalEntrega>> GetLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await GetLocaisEntregaAsync(connection, clienteEnderecoId, cancellationToken);
    }

    public async Task<IReadOnlyList<PrePedidoPDFTrocaItem>> GetTrocaItensAsync(
        int tblPrecoId,
        int estabelecimentoId,
        int segmentoId,
        int familiaId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(TrocaItensSql, connection);
        cmd.Parameters.AddWithValue("@TblPrecoID", tblPrecoId);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);
        cmd.Parameters.AddWithValue("@SegmentoID", segmentoId);
        cmd.Parameters.AddWithValue("@FamiliaID", familiaId);
        cmd.Parameters.AddWithValue("@ItemID", itemId);

        var items = new List<PrePedidoPDFTrocaItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFTrocaItem
            {
                CdItem = GetString(reader, "CdItem"),
                NmItem = GetString(reader, "NmItem"),
                ItemID = GetInt32(reader, "ItemID"),
                VlrTabelaPreco = GetDecimal(reader, "VlrTabelaPreco"),
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<PrePedidoPDFCatalogoItem>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(BuscarCatalogoSql, connection);
        cmd.Parameters.AddWithValue("@Descricao", descricao ?? string.Empty);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);
        cmd.Parameters.AddWithValue("@TblPrecoID", tblPrecoId);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);

        var items = new List<PrePedidoPDFCatalogoItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFCatalogoItem
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
                VlrCustoAquisicao = GetString(reader, "VlrCustoAquisicao"),
                VlrCustoMedio = GetString(reader, "VlrCustoMedio"),
                VlrTabela = GetDecimal(reader, "VlrTabela"),
                Criticidade = GetString(reader, "Criticidade"),
                TabelaPreco = GetString(reader, "TabelaPreco"),
                ItemDePara = GetString(reader, "ItemDePara"),
            });
        }

        return items;
    }

    private static async Task<PrePedidoPDFDetalhe?> GetDetalheAsync(
        SqlConnection connection,
        int id,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(DetalheSql, connection);
        cmd.Parameters.AddWithValue("@PDFPrePedidoID", id);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new PrePedidoPDFDetalhe
        {
            PDFPrePedidoPDFID = GetInt32(reader, "PDFPrePedidoID"),
            Arquivo = GetString(reader, "Arquivo"),
            ArquivoFormat = GetString(reader, "ArquivoFormat"),
            OrdemCompraDataHoraFormat = GetString(reader, "OrdemCompraDataHoraFormat"),
            CadastroUsuarioID = GetInt32(reader, "CadastroUsuarioID"),
            CadastroNmUsuario = GetString(reader, "CadastroNmUsuario"),
            StatusPrePedidoPDFID = GetInt32(reader, "StatusPrePedidoID"),
            StatusDescricao = GetString(reader, "StatusDescricao"),
            CotacaoID = GetInt32(reader, "CotacaoID"),
            OrdemCompra = GetString(reader, "OrdemCompra"),
            CNPJ = GetString(reader, "CNPJ"),
            ClienteLocalEntregaID = GetInt32(reader, "ClienteLocalEntregaID"),
            ClienteEnderecoID = GetInt32(reader, "ClienteEnderecoID"),
            Cliente = GetString(reader, "Cliente"),
            Estabelecimento = GetString(reader, "Estabelecimento"),
            EstabelecimentoID = GetInt32(reader, "EstabelecimentoID"),
            Endereco = GetString(reader, "Endereco"),
            NmLocalEntrega = GetString(reader, "NmLocalEntrega"),
            CondPagto = GetString(reader, "CondPagto"),
            CanalVenda = GetString(reader, "CanalVenda"),
            TipoOVSAP = GetString(reader, "TipoOVSAP"),
            TabelaPreco = GetString(reader, "TabelaPreco"),
            CdExtCliente = GetString(reader, "CdExtCliente"),
            ClienteID = GetInt32(reader, "ClienteID"),
            TblPrecoID = GetInt32(reader, "TblPrecoID"),
            LogoCliente = GetString(reader, "LogoCliente"),
            NmCliente = GetString(reader, "NmCliente"),
            VlrMinimoBloqueioPedido = GetDecimal(reader, "VlrMinimoBloqueioPedido"),
            ObsNota = GetString(reader, "ObsNota"),
            ObsComprador = GetString(reader, "ObsComprador"),
            ClienteCategoriaPedidoID = GetNullableInt32(reader, "ClienteCategoriaPedidoID"),
            NmCategoriaPedido = GetString(reader, "NmCategoriaPedido"),
        };
    }

    private static async Task<IReadOnlyList<PrePedidoPDFItem>> GetItensAsync(
        SqlConnection connection,
        int id,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(ItensSql, connection);
        cmd.Parameters.AddWithValue("@PDFPrePedidoID", id);

        var items = new List<PrePedidoPDFItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFItem
            {
                PDFPrePedidoPDFItemID = GetInt32(reader, "PDFPrePedidoItemID"),
                PDFPrePedidoPDFID = GetInt32(reader, "PDFPrePedidoID"),
                PDFSeqItem = GetInt32(reader, "PDFSeqItem"),
                PDFQtde = GetInt32(reader, "PDFQtde"),
                ItemInternoID = GetInt32(reader, "ItemInternoID"),
                ItemCliente = GetString(reader, "ItemCliente"),
                Descricao = GetString(reader, "Descricao"),
                ItemID = GetString(reader, "ItemID"),
                ItemBrSupply = GetString(reader, "ItemBrSupply"),
                SegmentoID = GetInt32(reader, "SegmentoID"),
                FamiliaID = GetInt32(reader, "FamiliaID"),
                VlrTblPrecoFormat = GetString(reader, "VlrTblPrecoFormat"),
                PDFVlrUnit = GetString(reader, "PDFVlrUnit"),
                VlrTotal = GetString(reader, "VlrTotal"),
                VlrTotalPedido = GetString(reader, "VlrTotalPedido"),
            });
        }

        return items;
    }

    private static async Task<IReadOnlyList<PrePedidoPDFLog>> GetLogsAsync(
        SqlConnection connection,
        int id,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(LogsSql, connection);
        cmd.Parameters.AddWithValue("@PDFPrePedidoID", id);

        var items = new List<PrePedidoPDFLog>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFLog
            {
                Mensagem = GetString(reader, "Mensagem"),
                CriadoEmFormatado = GetString(reader, "CriadoEmFormatado"),
                Tipo = GetString(reader, "Tipo"),
            });
        }

        return items;
    }

    private static async Task<IReadOnlyList<PrePedidoPDFEndereco>> GetEnderecosAsync(
        SqlConnection connection,
        int clienteId,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(EnderecosSql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var items = new List<PrePedidoPDFEndereco>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFEndereco
            {
                ClienteEnderecoID = GetInt32(reader, "ClienteEnderecoID"),
                Logradouro = GetString(reader, "Logradouro"),
            });
        }

        return items;
    }

    private static async Task<IReadOnlyList<PrePedidoPDFLocalEntrega>> GetLocaisEntregaAsync(
        SqlConnection connection,
        int clienteEnderecoId,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(LocaisEntregaSql, connection);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);

        var items = new List<PrePedidoPDFLocalEntrega>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFLocalEntrega
            {
                ClienteLocalEntregaID = GetInt32(reader, "ClienteLocalEntregaID"),
                NmLocalEntrega = GetString(reader, "NmLocalEntrega"),
                CdControle = GetString(reader, "CdControle"),
            });
        }

        return items;
    }

    private static async Task<IReadOnlyList<PrePedidoPDFCnpj>> GetCnpjsAsync(
        SqlConnection connection,
        int clienteId,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(CnpjsSql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var items = new List<PrePedidoPDFCnpj>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFCnpj
            {
                ClienteEnderecoID = GetInt32(reader, "ClienteEnderecoID"),
                CPFCNPJ = GetString(reader, "CPFCNPJ"),
            });
        }

        return items;
    }

    private static async Task<int> GetQtdLogsErroAsync(
        SqlConnection connection,
        int id,
        CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(LogsErroSql, connection);
        cmd.Parameters.AddWithValue("@PDFPrePedidoID", id);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static int GetInt32(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static decimal GetDecimal(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(reader.GetValue(ordinal));
    }

    private static string GetString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetValue(ordinal).ToString() ?? string.Empty;
    }

    private static int? GetNullableInt32(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private const string InfoGerarPedidoSql = """
        SELECT PDF.EstabelecimentoID,
               PDF.ClienteID,
               PDF.ClienteEnderecoID,
               PDF.CNPJ,
               PDF.ClienteLocalEntregaID,
               PPC.ClienteUsuarioID,
               ISNULL(PDF.NaturezaOperacaoID, 1) AS NaturezaOperacaoID,
               PDF.CondPagtoID,
               PDF.OrdemCompra,
               PDF.ClienteCategoriaPedidoID
        FROM Integracao_Clientes.dbo.PDF_PrePedido PDF WITH (NOLOCK)
        LEFT JOIN Integracao_Clientes.dbo.PPedido_ProcessadorPedidoConfiguracao PPC WITH (NOLOCK) ON PPC.ClienteID = PDF.ClienteID
        WHERE PDF.PDFPrePedidoID = @PDFPrePedidoID
        """;

    private const string InfoItensGerarPedidoSql = """
        SELECT PEDIDO.CotacaoID AS CotacaoID,
               1 AS Tipo,
               ITEM.ItemID AS ItemID,
               ITEM.Quantidade AS QtItem,
               ITEM.ValorUnitario AS VlrUnit,
               ITEM.CdItemCliente AS CdItemCliente,
               ITEM.OrdemCliente AS OrdemCliente,
               ITEM.Sequencia AS SeqCliente
        FROM Integracao_Clientes.dbo.PDF_PrePedidoItem AS ITEM WITH (NOLOCK)
        LEFT JOIN Integracao_Clientes.dbo.PDF_PrePedido AS PEDIDO WITH (NOLOCK) ON PEDIDO.PDFPrePedidoID = ITEM.PDFPrePedidoID
        WHERE ITEM.PDFPrePedidoID = @PDFPrePedidoID
        """;

    public async Task<PrePedidoPDFInfoGerarPedido?> GetInfoGerarPedidoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(InfoGerarPedidoSql, connection);
        cmd.Parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new PrePedidoPDFInfoGerarPedido
        {
            EstabelecimentoID = GetInt32(reader, "EstabelecimentoID"),
            ClienteID = GetInt32(reader, "ClienteID"),
            ClienteEnderecoID = GetInt32(reader, "ClienteEnderecoID"),
            CNPJ = GetString(reader, "CNPJ"),
            ClienteLocalEntregaID = GetInt32(reader, "ClienteLocalEntregaID"),
            ClienteUsuarioID = GetInt32(reader, "ClienteUsuarioID"),
            NaturezaOperacaoID = GetInt32(reader, "NaturezaOperacaoID"),
            CondPagtoID = GetInt32(reader, "CondPagtoID"),
            OrdemCompra = GetString(reader, "OrdemCompra"),
            ClienteCategoriaPedidoID = GetNullableInt32(reader, "ClienteCategoriaPedidoID"),
        };
    }

    public async Task<IReadOnlyList<PrePedidoPDFInfoItemGerarPedido>> GetInfoItensGerarPedidoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(InfoItensGerarPedidoSql, connection);
        cmd.Parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);

        var items = new List<PrePedidoPDFInfoItemGerarPedido>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PrePedidoPDFInfoItemGerarPedido
            {
                CotacaoID = GetInt32(reader, "CotacaoID"),
                Tipo = GetInt32(reader, "Tipo"),
                ItemID = GetInt32(reader, "ItemID"),
                QtItem = GetInt32(reader, "QtItem"),
                VlrUnit = GetDecimal(reader, "VlrUnit"),
                CdItemCliente = GetString(reader, "CdItemCliente"),
                OrdemCliente = GetString(reader, "OrdemCliente"),
                SeqCliente = GetInt32(reader, "SeqCliente"),
            });
        }

        return items;
    }
}
