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
}
