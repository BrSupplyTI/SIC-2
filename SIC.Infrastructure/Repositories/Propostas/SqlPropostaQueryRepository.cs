using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.Propostas;
using SIC.Domain.Entities.Propostas;
using System.Text;

namespace SIC.Infrastructure.Repositories.Propostas;

public sealed class SqlPropostaQueryRepository(IConfiguration configuration) : IPropostaQueryRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<PropostaListItem>> GetListAsync(
        string? filtroCodigo,
        string? filtroNome,
        string? filtroEstabelecimento,
        string? filtroStatus,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            SELECT NNP.PropostaID,
                   FORMAT(NNP.DtCriacao, 'dd/MM/yyyy') AS DtCriacao,
                   NNP.EstabelecimentoID,
                   E.NmEstabelecimento,
                   NNP.NomeProposta,
                   NNP.StatusID,
                   SP.NmStatus,
                   ISNULL(NPIT.TotalItens, 0) AS TotalItens,
                   ISNULL(NPIT.ItensProcessados, 0) AS ItensProcessados,
                   ISNULL(NPIT.PercentualConcluido, '0%') AS PercentualConcluido
            FROM BrWeb.dbo.NovosNegocios_Proposta NNP WITH (NOLOCK)
            LEFT JOIN BrWeb.dbo.NovosNegocios_StatusProposta SP WITH (NOLOCK) ON SP.StatusID = NNP.StatusID
            LEFT JOIN BrSupply.dbo.BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = NNP.EstabelecimentoID
            LEFT JOIN (
                SELECT
                    PropostaID,
                    COUNT(*) AS TotalItens,
                    SUM(CASE WHEN ItemID IS NOT NULL OR FlagSemCorrespondencia = 1 THEN 1 ELSE 0 END) AS ItensProcessados,
                    CONVERT(VARCHAR(10),
                        CAST(
                            (CAST(SUM(CASE WHEN ItemID IS NOT NULL OR FlagSemCorrespondencia = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2))
                             / NULLIF(COUNT(*), 0)) * 100
                        AS INT)
                    ) + '%' AS PercentualConcluido
                FROM BrWeb.dbo.NovosNegocios_PropostaItem WITH (NOLOCK)
                GROUP BY PropostaID
            ) NPIT ON NPIT.PropostaID = NNP.PropostaID
            WHERE 1 = 1
            """);

        if (!string.IsNullOrWhiteSpace(filtroCodigo))
            sb.AppendLine("AND NNP.PropostaID = @FiltroCodigo");

        if (!string.IsNullOrWhiteSpace(filtroNome))
            sb.AppendLine("AND NNP.NomeProposta LIKE '%' + @FiltroNome + '%'");

        if (!string.IsNullOrWhiteSpace(filtroEstabelecimento))
            sb.AppendLine("AND E.NmEstabelecimento LIKE '%' + @FiltroEstabelecimento + '%'");

        if (!string.IsNullOrWhiteSpace(filtroStatus))
            sb.AppendLine("AND SP.NmStatus LIKE '%' + @FiltroStatus + '%'");

        sb.AppendLine("ORDER BY NNP.PropostaID DESC");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sb.ToString(), connection);

        if (!string.IsNullOrWhiteSpace(filtroCodigo) && int.TryParse(filtroCodigo, out var codigoInt))
            cmd.Parameters.AddWithValue("@FiltroCodigo", codigoInt);

        if (!string.IsNullOrWhiteSpace(filtroNome))
            cmd.Parameters.AddWithValue("@FiltroNome", filtroNome);

        if (!string.IsNullOrWhiteSpace(filtroEstabelecimento))
            cmd.Parameters.AddWithValue("@FiltroEstabelecimento", filtroEstabelecimento);

        if (!string.IsNullOrWhiteSpace(filtroStatus))
            cmd.Parameters.AddWithValue("@FiltroStatus", filtroStatus);

        var items = new List<PropostaListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PropostaListItem
            {
                PropostaID = reader.GetInt32(reader.GetOrdinal("PropostaID")),
                NomeProposta = reader.IsDBNull(reader.GetOrdinal("NomeProposta")) ? string.Empty : reader.GetString(reader.GetOrdinal("NomeProposta")),
                EstabelecimentoID = reader.IsDBNull(reader.GetOrdinal("EstabelecimentoID")) ? 0 : reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                NmEstabelecimento = reader.IsDBNull(reader.GetOrdinal("NmEstabelecimento")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmEstabelecimento")),
                DtCriacao = reader.IsDBNull(reader.GetOrdinal("DtCriacao")) ? string.Empty : reader.GetString(reader.GetOrdinal("DtCriacao")),
                StatusID = reader.IsDBNull(reader.GetOrdinal("StatusID")) ? 0 : reader.GetInt32(reader.GetOrdinal("StatusID")),
                NmStatus = reader.IsDBNull(reader.GetOrdinal("NmStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmStatus")),
                TotalItens = reader.IsDBNull(reader.GetOrdinal("TotalItens")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalItens")),
                ItensProcessados = reader.IsDBNull(reader.GetOrdinal("ItensProcessados")) ? 0 : reader.GetInt32(reader.GetOrdinal("ItensProcessados")),
                PercentualConcluido = reader.IsDBNull(reader.GetOrdinal("PercentualConcluido")) ? "0%" : reader.GetString(reader.GetOrdinal("PercentualConcluido")),
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<SegmentoItem>> GetSegmentosAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            SELECT SegmentoID, NmSegmento
            FROM BrSupply.dbo.BR_Segmento
            WHERE FlagPadraoBrSupply = 1
              AND FlagAtivo = 1
            ORDER BY NmSegmento ASC
            """, connection);

        var items = new List<SegmentoItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SegmentoItem
            {
                SegmentoID = reader.GetInt32(reader.GetOrdinal("SegmentoID")),
                NmSegmento = reader.IsDBNull(reader.GetOrdinal("NmSegmento")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmSegmento")),
            });
        }

        return items;
    }

    public async Task<int> SalvarPropostaAsync(
        int estabelecimentoId,
        string nomeProposta,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            INSERT INTO BrWeb.dbo.NovosNegocios_Proposta
                (EstabelecimentoID, NomeProposta, StatusID, DtCriacao)
            VALUES
                (@EstabelecimentoID, @NomeProposta, 1, GETDATE());
            SELECT SCOPE_IDENTITY();
            """, connection);

        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);
        cmd.Parameters.AddWithValue("@NomeProposta", nomeProposta);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    public async Task SalvarPropostaQualidadeAsync(
        int propostaId,
        int segmentoId,
        string qualidade,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            INSERT INTO BrWeb.dbo.NovosNegocios_PropostaQualidade
                (PropostaID, SegmentoID, Qualidade)
            VALUES
                (@PropostaID, @SegmentoID, @Qualidade)
            """, connection);

        cmd.Parameters.AddWithValue("@PropostaID", propostaId);
        cmd.Parameters.AddWithValue("@SegmentoID", segmentoId);
        cmd.Parameters.AddWithValue("@Qualidade", qualidade);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<PropostaDetalhe?> GetByIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            SELECT PropostaID, EstabelecimentoID, NomeProposta, StatusID
            FROM BrWeb.dbo.NovosNegocios_Proposta WITH (NOLOCK)
            WHERE PropostaID = @PropostaID
            """, connection);

        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var detalhe = new PropostaDetalhe
        {
            PropostaID = reader.GetInt32(reader.GetOrdinal("PropostaID")),
            EstabelecimentoID = reader.IsDBNull(reader.GetOrdinal("EstabelecimentoID")) ? 0 : reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
            NomeProposta = reader.IsDBNull(reader.GetOrdinal("NomeProposta")) ? string.Empty : reader.GetString(reader.GetOrdinal("NomeProposta")),
            StatusID = reader.IsDBNull(reader.GetOrdinal("StatusID")) ? 0 : reader.GetInt32(reader.GetOrdinal("StatusID")),
        };

        await reader.CloseAsync();

        await using var cmdQs = new SqlCommand("""
            SELECT PQ.SegmentoID, S.NmSegmento, PQ.Qualidade
            FROM BrWeb.dbo.NovosNegocios_PropostaQualidade PQ WITH (NOLOCK)
            INNER JOIN BrSupply.dbo.BR_Segmento S WITH (NOLOCK) ON S.SegmentoID = PQ.SegmentoID
            WHERE PQ.PropostaID = @PropostaID
            """, connection);

        cmdQs.Parameters.AddWithValue("@PropostaID", propostaId);

        await using var readerQs = await cmdQs.ExecuteReaderAsync(cancellationToken);
        while (await readerQs.ReadAsync(cancellationToken))
        {
            detalhe.QualSeg.Add(new PropostaQualSegItem
            {
                SegmentoID = readerQs.GetInt32(readerQs.GetOrdinal("SegmentoID")),
                NmSegmento = readerQs.IsDBNull(readerQs.GetOrdinal("NmSegmento")) ? string.Empty : readerQs.GetString(readerQs.GetOrdinal("NmSegmento")),
                Qualidade = readerQs.IsDBNull(readerQs.GetOrdinal("Qualidade")) ? string.Empty : readerQs.GetString(readerQs.GetOrdinal("Qualidade")),
            });
        }

        return detalhe;
    }

    public async Task AtualizarPropostaAsync(
        int propostaId,
        int estabelecimentoId,
        string nomeProposta,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            UPDATE BrWeb.dbo.NovosNegocios_Proposta
            SET EstabelecimentoID = @EstabelecimentoID,
                NomeProposta = @NomeProposta
            WHERE PropostaID = @PropostaID
            """, connection);

        cmd.Parameters.AddWithValue("@PropostaID", propostaId);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);
        cmd.Parameters.AddWithValue("@NomeProposta", nomeProposta);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeletarPropostaQualidadesAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            DELETE FROM BrWeb.dbo.NovosNegocios_PropostaQualidade
            WHERE PropostaID = @PropostaID
            """, connection);

        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<PropostaCodificacao?> GetCodificacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 1) Proposta header
        await using var cmdProposta = new SqlCommand("""
            SELECT NNP.PropostaID, NNP.CdProposta, NNP.EstabelecimentoID,
                   E.NmEstabelecimento, NNP.NomeProposta, NNP.StatusID,
                   SP.NmStatus, ISNULL(NNP.MargemPadrao, 0) AS MargemPadrao,
                   ISNULL((SELECT COUNT(*) FROM BrWeb.dbo.NovosNegocios_PropostaItem NNPI WITH (NOLOCK)
                           WHERE NNPI.PropostaID = NNP.PropostaID), 0) AS TotalItens
            FROM BrWeb.dbo.NovosNegocios_Proposta NNP WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = NNP.EstabelecimentoID
            LEFT JOIN BrWeb.dbo.NovosNegocios_StatusProposta SP WITH (NOLOCK) ON SP.StatusID = NNP.StatusID
            WHERE NNP.PropostaID = @PropostaID
            """, connection);
        cmdProposta.Parameters.AddWithValue("@PropostaID", propostaId);

        PropostaCodificacao? proposta = null;
        await using (var reader = await cmdProposta.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                proposta = new PropostaCodificacao
                {
                    PropostaID = reader.GetInt32(reader.GetOrdinal("PropostaID")),
                    CdProposta = reader.IsDBNull(reader.GetOrdinal("CdProposta")) ? string.Empty : reader.GetString(reader.GetOrdinal("CdProposta")),
                    EstabelecimentoID = reader.IsDBNull(reader.GetOrdinal("EstabelecimentoID")) ? 0 : reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                    NmEstabelecimento = reader.IsDBNull(reader.GetOrdinal("NmEstabelecimento")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmEstabelecimento")),
                    NomeProposta = reader.IsDBNull(reader.GetOrdinal("NomeProposta")) ? string.Empty : reader.GetString(reader.GetOrdinal("NomeProposta")),
                    StatusID = reader.IsDBNull(reader.GetOrdinal("StatusID")) ? 0 : reader.GetInt32(reader.GetOrdinal("StatusID")),
                    NmStatus = reader.IsDBNull(reader.GetOrdinal("NmStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmStatus")),
                    MargemPadrao = reader.GetDecimal(reader.GetOrdinal("MargemPadrao")),
                    TotalItens = reader.GetInt32(reader.GetOrdinal("TotalItens")),
                };
            }
        }

        if (proposta is null) return null;

        // 2) Percentual concluído
        await using var cmdConcluido = new SqlCommand("""
            SELECT CONVERT(VARCHAR(10),
                CAST(
                    (CAST(SUM(CASE WHEN ItemID IS NOT NULL OR FlagSemCorrespondencia = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2))
                     / NULLIF(COUNT(*), 0)) * 100
                AS INT)
            ) + '%' AS PercentualConcluido
            FROM BrWeb.dbo.NovosNegocios_PropostaItem WITH (NOLOCK)
            WHERE PropostaID = @PropostaID
            """, connection);
        cmdConcluido.Parameters.AddWithValue("@PropostaID", propostaId);

        var percentual = await cmdConcluido.ExecuteScalarAsync(cancellationToken);
        proposta.PercentualConcluido = percentual is string s ? s : "0%";

        // 3) QualSeg
        await using var cmdQs = new SqlCommand("""
            SELECT S.NmSegmento,
                   CASE NNP.Qualidade
                       WHEN 'P' THEN 'Premium'
                       WHEN 'B' THEN 'Básico'
                       WHEN 'I' THEN 'Intermediário'
                       ELSE 'Desconhecido'
                   END AS Qualidade
            FROM BrWeb.dbo.NovosNegocios_PropostaQualidade NNP WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Segmento S WITH (NOLOCK) ON S.SegmentoID = NNP.SegmentoID
            WHERE NNP.PropostaID = @PropostaID
            """, connection);
        cmdQs.Parameters.AddWithValue("@PropostaID", propostaId);

        await using (var readerQs = await cmdQs.ExecuteReaderAsync(cancellationToken))
        {
            while (await readerQs.ReadAsync(cancellationToken))
            {
                proposta.QualSeg.Add(new PropostaQualSegItem
                {
                    NmSegmento = readerQs.IsDBNull(readerQs.GetOrdinal("NmSegmento")) ? string.Empty : readerQs.GetString(readerQs.GetOrdinal("NmSegmento")),
                    Qualidade = readerQs.IsDBNull(readerQs.GetOrdinal("Qualidade")) ? string.Empty : readerQs.GetString(readerQs.GetOrdinal("Qualidade")),
                });
            }
        }

        // 4) Itens
        await using var cmdItens = new SqlCommand("""
            SELECT NNPI.PropostaItemID, NNPI.PropostaID,
                   NNPI.DescricaoBreve, NNPI.NumeroCA,
                   NNPI.MarcaFornecedor AS NmMarca,
                   NNPI.ItemID,
                   CASE WHEN NNPI.FlagForaDeMix = 1 THEN 'Item Fora do Mix' ELSE I.CdItem END AS CdItem,
                   CASE WHEN NNPI.FlagForaDeMix = 1 THEN 'Item Fora do Mix' ELSE I.NmItem END AS NmItem,
                   PM.Qualidade,
                   FORMAT(PE.VlrCustoAquisicao, 'N', 'pt-br') AS VlrCustoAquisicaoFormat,
                   ISNULL(NNPI.FlagForaDeMix, 0) AS FlagForaDeMix,
                   ISNULL(NNPI.FlagSemCorrespondencia, 0) AS FlagSemCorrespondencia,
                   ISNULL(NNPI.FlagAddManual, 0) AS FlagAddManual,
                   NNPI.CodCliente, NNPI.DescricaoDetalhada, NNPI.Familia,
                   NNPI.MarcaFornecedor, NNPI.UnidadeMedida,
                   NNPI.Target,
                   FORMAT(NNPI.Target, 'C', 'pt-br') AS TargetFormat,
                   NNPI.QtdAnual
            FROM BrWeb.dbo.NovosNegocios_PropostaItem NNPI WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Item I WITH (NOLOCK) ON I.ItemID = NNPI.ItemID
            LEFT JOIN BrWeb.dbo.NovosNegocios_Proposta NNP WITH (NOLOCK) ON NNP.PropostaID = NNPI.PropostaID
            LEFT JOIN BrSupply.dbo.BR_PrecoEstoque PE WITH (NOLOCK) ON PE.ItemID = NNPI.ItemID AND PE.EstabelecimentoID = NNP.EstabelecimentoID
            LEFT JOIN BrSupply.dbo.BR_ProdutoMarca PM WITH (NOLOCK) ON PM.ProdutoMarcaID = I.ProdutoMarcaID
            WHERE NNPI.PropostaID = @PropostaID
            ORDER BY NNPI.DescricaoBreve ASC
            """, connection);
        cmdItens.Parameters.AddWithValue("@PropostaID", propostaId);

        await using (var readerItens = await cmdItens.ExecuteReaderAsync(cancellationToken))
        {
            while (await readerItens.ReadAsync(cancellationToken))
            {
                proposta.Itens.Add(new PropostaCodificacaoItem
                {
                    PropostaItemID = readerItens.GetInt32(readerItens.GetOrdinal("PropostaItemID")),
                    PropostaID = readerItens.GetInt32(readerItens.GetOrdinal("PropostaID")),
                    DescricaoBreve = readerItens.IsDBNull(readerItens.GetOrdinal("DescricaoBreve")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("DescricaoBreve")),
                    NumeroCA = readerItens.IsDBNull(readerItens.GetOrdinal("NumeroCA")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("NumeroCA")),
                    NmMarca = readerItens.IsDBNull(readerItens.GetOrdinal("NmMarca")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("NmMarca")),
                    ItemID = readerItens.IsDBNull(readerItens.GetOrdinal("ItemID")) ? null : readerItens.GetInt32(readerItens.GetOrdinal("ItemID")),
                    CdItem = readerItens.IsDBNull(readerItens.GetOrdinal("CdItem")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("CdItem")),
                    NmItem = readerItens.IsDBNull(readerItens.GetOrdinal("NmItem")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("NmItem")),
                    Qualidade = readerItens.IsDBNull(readerItens.GetOrdinal("Qualidade")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("Qualidade")),
                    VlrCustoAquisicaoFormat = readerItens.IsDBNull(readerItens.GetOrdinal("VlrCustoAquisicaoFormat")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("VlrCustoAquisicaoFormat")),
                    FlagForaDeMix = readerItens.GetInt32(readerItens.GetOrdinal("FlagForaDeMix")) == 1,
                    FlagSemCorrespondencia = readerItens.GetInt32(readerItens.GetOrdinal("FlagSemCorrespondencia")) == 1,
                    FlagAddManual = readerItens.GetInt32(readerItens.GetOrdinal("FlagAddManual")) == 1,
                    CodCliente = readerItens.IsDBNull(readerItens.GetOrdinal("CodCliente")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("CodCliente")),
                    DescricaoDetalhada = readerItens.IsDBNull(readerItens.GetOrdinal("DescricaoDetalhada")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("DescricaoDetalhada")),
                    Familia = readerItens.IsDBNull(readerItens.GetOrdinal("Familia")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("Familia")),
                    MarcaFornecedor = readerItens.IsDBNull(readerItens.GetOrdinal("MarcaFornecedor")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("MarcaFornecedor")),
                    UnidadeMedida = readerItens.IsDBNull(readerItens.GetOrdinal("UnidadeMedida")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("UnidadeMedida")),
                    Target = readerItens.IsDBNull(readerItens.GetOrdinal("Target")) ? null : readerItens.GetDecimal(readerItens.GetOrdinal("Target")),
                    TargetFormat = readerItens.IsDBNull(readerItens.GetOrdinal("TargetFormat")) ? string.Empty : readerItens.GetString(readerItens.GetOrdinal("TargetFormat")),
                    QtdAnual = readerItens.IsDBNull(readerItens.GetOrdinal("QtdAnual")) ? null : readerItens.GetInt32(readerItens.GetOrdinal("QtdAnual")),
                });
            }
        }

        return proposta;
    }

    public async Task<IReadOnlyList<ItemBuscaResult>> BuscarItensBrSupplyAsync(
        int estabelecimentoId,
        string filtro,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = """
            SET NOCOUNT ON
            DECLARE @TblResult TABLE (
                ItemID INT,
                FlagTipo INT,
                Prioridade INT,
                Probabilidade INT,
                CdItem VARCHAR(10),
                NmItem VARCHAR(100),
                NmFornecedor VARCHAR(300),
                ProdutoMarcaID INT,
                Marca VARCHAR(60),
                PathFoto VARCHAR(10),
                ClienteID INT,
                Tipo INT,
                VlrUnit DECIMAL(12,2)
            )
            INSERT INTO @TblResult EXEC BrSupply..BRS_sp_PesquisaCatalogo_V2 @Filtro, 1, 0, 0, 0, 1, 0, 1, 100, 0, 0

            SELECT T.ItemID,
                   T.Probabilidade,
                   I.CdItem,
                   I.NmItem,
                   T.ProdutoMarcaID,
                   T.Marca,
                   ISNULL(M.Qualidade, 'Não especificado') AS Qualidade,
                   FORMAT(PE.VlrCustoAquisicao, 'N', 'pt-br') AS VlrCustoAquisicaoFormat
            FROM @TblResult T
            INNER JOIN BrSupply.dbo.BR_Item I WITH (NOLOCK) ON I.ItemID = T.ItemID
            INNER JOIN BrSupply.dbo.BR_PrecoEstoque PE WITH (NOLOCK) ON PE.ItemID = I.ItemID AND PE.EstabelecimentoID = @EstabelecimentoID
            LEFT JOIN BrSupply.dbo.BR_ProdutoMarca M ON M.ProdutoMarcaID = T.ProdutoMarcaID
            WHERE I.FlagAtivo = 1
              AND ISNULL(PE.FlagOutlet, 0) = 0
            ORDER BY T.Probabilidade DESC
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Filtro", filtro);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);

        var items = new List<ItemBuscaResult>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ItemBuscaResult
            {
                ItemID = reader.GetInt32(reader.GetOrdinal("ItemID")),
                Probabilidade = reader.IsDBNull(reader.GetOrdinal("Probabilidade")) ? 0 : reader.GetInt32(reader.GetOrdinal("Probabilidade")),
                CdItem = reader.IsDBNull(reader.GetOrdinal("CdItem")) ? string.Empty : reader.GetString(reader.GetOrdinal("CdItem")),
                NmItem = reader.IsDBNull(reader.GetOrdinal("NmItem")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmItem")),
                Qualidade = reader.IsDBNull(reader.GetOrdinal("Qualidade")) ? string.Empty : reader.GetString(reader.GetOrdinal("Qualidade")),
                VlrCustoAquisicaoFormat = reader.IsDBNull(reader.GetOrdinal("VlrCustoAquisicaoFormat")) ? string.Empty : reader.GetString(reader.GetOrdinal("VlrCustoAquisicaoFormat")),
            });
        }

        return items;
    }

    public async Task<bool> AdicionarItemPropostaAsync(
        int propostaId,
        int itemId,
        int qtdAnual,
        decimal margemPadrao,
        string descricaoBreve,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            INSERT INTO BrWeb.dbo.NovosNegocios_PropostaItem
                (DescricaoBreve, PropostaID, ItemID, QtdAnual, MargemDefinida, TipoCusto, FlagAddManual, Quantidade)
            VALUES
                (@DescricaoBreve, @PropostaID, @ItemID, @QtdAnual, @MargemDefinida, 'aqs', 1, @QtdAnual)
            """, connection);

        cmd.Parameters.AddWithValue("@DescricaoBreve", descricaoBreve);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);
        cmd.Parameters.AddWithValue("@ItemID", itemId);
        cmd.Parameters.AddWithValue("@QtdAnual", qtdAnual);
        cmd.Parameters.AddWithValue("@MargemDefinida", margemPadrao);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> ExcluirItemPropostaAsync(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            DELETE FROM BrWeb.dbo.NovosNegocios_PropostaItem
            WHERE PropostaID = @PropostaID AND PropostaItemID = @PropostaItemID
            """, connection);

        cmd.Parameters.AddWithValue("@PropostaID", propostaId);
        cmd.Parameters.AddWithValue("@PropostaItemID", propostaItemId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<int> ImportarItensAsync(
        int propostaId,
        IReadOnlyList<(string CodCliente, string DescricaoBreve, string DescricaoDetalhada, string Familia, string MarcaFornecedor, string UnidadeMedida, int QtdAnual, decimal Target)> itens,
        CancellationToken cancellationToken = default)
    {
        if (itens.Count == 0) return 0;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var inserted = 0;

        foreach (var item in itens)
        {
            await using var cmd = new SqlCommand("""
                INSERT INTO BrWeb.dbo.NovosNegocios_PropostaItem
                    (PropostaID, CodCliente, DescricaoBreve, DescricaoDetalhada, Familia, MarcaFornecedor, UnidadeMedida, QtdAnual, Quantidade, Target, TipoCusto, FlagAddManual)
                VALUES
                    (@PropostaID, @CodCliente, @DescricaoBreve, @DescricaoDetalhada, @Familia, @MarcaFornecedor, @UnidadeMedida, @QtdAnual, @QtdAnual, @Target, 'aqs', 0)
                """, connection);

            cmd.Parameters.AddWithValue("@PropostaID", propostaId);
            cmd.Parameters.AddWithValue("@CodCliente", string.IsNullOrEmpty(item.CodCliente) ? DBNull.Value : item.CodCliente);
            cmd.Parameters.AddWithValue("@DescricaoBreve", string.IsNullOrEmpty(item.DescricaoBreve) ? DBNull.Value : item.DescricaoBreve);
            cmd.Parameters.AddWithValue("@DescricaoDetalhada", string.IsNullOrEmpty(item.DescricaoDetalhada) ? DBNull.Value : item.DescricaoDetalhada);
            cmd.Parameters.AddWithValue("@Familia", string.IsNullOrEmpty(item.Familia) ? DBNull.Value : item.Familia);
            cmd.Parameters.AddWithValue("@MarcaFornecedor", string.IsNullOrEmpty(item.MarcaFornecedor) ? DBNull.Value : item.MarcaFornecedor);
            cmd.Parameters.AddWithValue("@UnidadeMedida", string.IsNullOrEmpty(item.UnidadeMedida) ? DBNull.Value : item.UnidadeMedida);
            cmd.Parameters.AddWithValue("@QtdAnual", item.QtdAnual);
            cmd.Parameters.AddWithValue("@Target", item.Target);

            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
            inserted += rows;
        }

        return inserted;
    }

    public async Task<CodificarItemResult> CodificarItemAsync(
        int propostaItemId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 1) Buscar DescricaoBreve do item
        await using var cmdDesc = new SqlCommand("""
            SELECT DescricaoBreve
            FROM BrWeb.dbo.NovosNegocios_PropostaItem WITH (NOLOCK)
            WHERE PropostaItemID = @PropostaItemID
            """, connection);
        cmdDesc.Parameters.AddWithValue("@PropostaItemID", propostaItemId);

        var descricao = await cmdDesc.ExecuteScalarAsync(cancellationToken) as string;
        if (string.IsNullOrWhiteSpace(descricao))
        {
            return new CodificarItemResult
            {
                PropostaItemID = propostaItemId,
                Codificado = false,
                SemCorrespondencia = true
            };
        }

        // 2) Buscar melhor match no catálogo
        var sqlBusca = """
            SET NOCOUNT ON
            DECLARE @TblResult TABLE (
                ItemID INT, FlagTipo INT, Prioridade INT, Probabilidade INT,
                CdItem VARCHAR(10), NmItem VARCHAR(100), NmFornecedor VARCHAR(300),
                ProdutoMarcaID INT, Marca VARCHAR(60), PathFoto VARCHAR(10),
                ClienteID INT, Tipo INT, VlrUnit DECIMAL(12,2)
            )
            INSERT INTO @TblResult EXEC BrSupply..BRS_sp_PesquisaCatalogo_V2 @Filtro, 1, 0, 0, 0, 1, 0, 1, 1, 0, 0

            SELECT TOP 1 T.ItemID, I.CdItem, I.NmItem,
                   ISNULL(M.Qualidade, 'Não especificado') AS Qualidade
            FROM @TblResult T
            INNER JOIN BrSupply.dbo.BR_Item I WITH (NOLOCK) ON I.ItemID = T.ItemID
            INNER JOIN BrSupply.dbo.BR_PrecoEstoque PE WITH (NOLOCK) ON PE.ItemID = I.ItemID AND PE.EstabelecimentoID = @EstabelecimentoID
            LEFT JOIN BrSupply.dbo.BR_ProdutoMarca M ON M.ProdutoMarcaID = I.ProdutoMarcaID
            WHERE I.FlagAtivo = 1 AND ISNULL(PE.FlagOutlet, 0) = 0
            ORDER BY T.Probabilidade DESC
            """;

        await using var cmdBusca = new SqlCommand(sqlBusca, connection);
        cmdBusca.Parameters.AddWithValue("@Filtro", descricao);
        cmdBusca.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);

        int? matchedItemId = null;
        string cdItem = string.Empty, nmItem = string.Empty, qualidade = string.Empty;

        await using (var reader = await cmdBusca.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                matchedItemId = reader.GetInt32(reader.GetOrdinal("ItemID"));
                cdItem = reader.IsDBNull(reader.GetOrdinal("CdItem")) ? string.Empty : reader.GetString(reader.GetOrdinal("CdItem"));
                nmItem = reader.IsDBNull(reader.GetOrdinal("NmItem")) ? string.Empty : reader.GetString(reader.GetOrdinal("NmItem"));
                qualidade = reader.IsDBNull(reader.GetOrdinal("Qualidade")) ? string.Empty : reader.GetString(reader.GetOrdinal("Qualidade"));
            }
        }

        // 3) Atualizar o item
        if (matchedItemId.HasValue)
        {
            await using var cmdUpdate = new SqlCommand("""
                UPDATE BrWeb.dbo.NovosNegocios_PropostaItem
                SET ItemID = @ItemID, FlagSemCorrespondencia = 0
                WHERE PropostaItemID = @PropostaItemID
                """, connection);
            cmdUpdate.Parameters.AddWithValue("@ItemID", matchedItemId.Value);
            cmdUpdate.Parameters.AddWithValue("@PropostaItemID", propostaItemId);
            await cmdUpdate.ExecuteNonQueryAsync(cancellationToken);

            return new CodificarItemResult
            {
                PropostaItemID = propostaItemId,
                Codificado = true,
                SemCorrespondencia = false,
                ItemID = matchedItemId.Value,
                CdItem = cdItem,
                NmItem = nmItem,
                Qualidade = qualidade
            };
        }
        else
        {
            await using var cmdSem = new SqlCommand("""
                UPDATE BrWeb.dbo.NovosNegocios_PropostaItem
                SET FlagSemCorrespondencia = 1
                WHERE PropostaItemID = @PropostaItemID
                """, connection);
            cmdSem.Parameters.AddWithValue("@PropostaItemID", propostaItemId);
            await cmdSem.ExecuteNonQueryAsync(cancellationToken);

            return new CodificarItemResult
            {
                PropostaItemID = propostaItemId,
                Codificado = false,
                SemCorrespondencia = true
            };
        }
    }

    public async Task<bool> MarcarSegundoPlanoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            UPDATE BrWeb.dbo.NovosNegocios_Proposta
            SET StatusID = 9
            WHERE PropostaID = @PropostaID
            """, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<IReadOnlyList<(int PropostaID, int EstabelecimentoID)>> GetPropostasPendentesSegundoPlanoAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            SELECT PropostaID, EstabelecimentoID
            FROM BrWeb.dbo.NovosNegocios_Proposta WITH (NOLOCK)
            WHERE StatusID = 9
            """, connection);

        var items = new List<(int, int)>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add((
                reader.GetInt32(reader.GetOrdinal("PropostaID")),
                reader.IsDBNull(reader.GetOrdinal("EstabelecimentoID")) ? 0 : reader.GetInt32(reader.GetOrdinal("EstabelecimentoID"))
            ));
        }

        return items;
    }

    public async Task<IReadOnlyList<int>> GetItensNaoCodificadosAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            SELECT PropostaItemID
            FROM BrWeb.dbo.NovosNegocios_PropostaItem WITH (NOLOCK)
            WHERE PropostaID = @PropostaID
              AND ItemID IS NULL
              AND ISNULL(FlagSemCorrespondencia, 0) = 0
            """, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);

        var items = new List<int>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(reader.GetInt32(0));
        }

        return items;
    }

    public async Task AtualizarStatusPropostaAsync(
        int propostaId,
        int statusId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            UPDATE BrWeb.dbo.NovosNegocios_Proposta
            SET StatusID = @StatusID
            WHERE PropostaID = @PropostaID
            """, connection);
        cmd.Parameters.AddWithValue("@PropostaID", propostaId);
        cmd.Parameters.AddWithValue("@StatusID", statusId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ExcluirPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Deletar itens filhos primeiro (replica lógica do PHP: limparNovosNegociosPropostaItem)
        await using var cmdItens = new SqlCommand("""
            DELETE FROM BrWeb.dbo.NovosNegocios_PropostaItem
            WHERE PropostaID = @PropostaID
            """, connection);
        cmdItens.Parameters.AddWithValue("@PropostaID", propostaId);
        await cmdItens.ExecuteNonQueryAsync(cancellationToken);

        // Deletar qualidades da proposta
        await using var cmdQual = new SqlCommand("""
            DELETE FROM BrWeb.dbo.NovosNegocios_PropostaQualidade
            WHERE PropostaID = @PropostaID
            """, connection);
        cmdQual.Parameters.AddWithValue("@PropostaID", propostaId);
        await cmdQual.ExecuteNonQueryAsync(cancellationToken);

        // Deletar a proposta (replica lógica do PHP: parent::delete)
        await using var cmdProposta = new SqlCommand("""
            DELETE FROM BrWeb.dbo.NovosNegocios_Proposta
            WHERE PropostaID = @PropostaID
            """, connection);
        cmdProposta.Parameters.AddWithValue("@PropostaID", propostaId);

        var rows = await cmdProposta.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> VincularItemManualAsync(
        int propostaItemId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("""
            UPDATE BrWeb.dbo.NovosNegocios_PropostaItem
            SET ItemID = @ItemID, FlagSemCorrespondencia = 0
            WHERE PropostaItemID = @PropostaItemID
            """, connection);
        cmd.Parameters.AddWithValue("@ItemID", itemId);
        cmd.Parameters.AddWithValue("@PropostaItemID", propostaItemId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }
}
