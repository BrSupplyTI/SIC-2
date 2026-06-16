using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.Categorizacao;
using SIC.Domain.Entities.Categorizacao;

namespace SIC.Infrastructure.Repositories.Categorizacao;

public sealed class SqlCategorizacaoRepository(IConfiguration configuration, IMemoryCache cache) : ICategorizacaoRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    private static string CacheKeyItens(int? id) => $"cat_itens_{id ?? 0}";
    private const string CacheKeySemCategoria = "cat_sem_categoria";
    private const string CacheKeyCategorias   = "cat_tipo_lista";

    public async Task<IReadOnlyList<CategorizacaoItem>> GetItensCategorizadosAsync(
        int? estabelecimentoId, CancellationToken cancellationToken = default)
    {
        var key = CacheKeyItens(estabelecimentoId);
        if (cache.TryGetValue(key, out IReadOnlyList<CategorizacaoItem>? cached) && cached is not null)
            return cached;

        var filtro = estabelecimentoId is > 0
            ? $"AND PE.EstabelecimentoID = {estabelecimentoId.Value}"
            : "AND PE.EstabelecimentoID IN (1,9)";

        var sql = $"""
            SELECT
                E.EstabelecimentoID,
                E.NmEstabelecimento,
                I.ItemID,
                I.CdItem,
                I.NmItem,
                CASE
                    WHEN ISNULL(PE.FlagOutlet,0)=1     THEN 'Y'
                    WHEN ISNULL(PE.FlagSobDemanda,0)=1 THEN 'Z'
                    ELSE 'X'
                END AS Criticidade,
                PE.VlrCustoAquisicao,
                CONVERT(INT, PE.QtDispEstoque) AS QtDispEstoque,
                PTL.NmTipoLista,
                PTL.PesquisaTipoListaID,
                PIL.Prioridade
            FROM BrSupply.dbo.BR_PesquisaItemLista PIL WITH (NOLOCK)
            JOIN      BrSupply.dbo.BR_Item                I   WITH (NOLOCK) ON I.ItemID               = PIL.ItemID AND I.FlagAtivo = 1 AND I.FlagCatalogo = 1
            LEFT JOIN BrSupply.dbo.BR_PesquisaTipoLista   PTL WITH (NOLOCK) ON PTL.PesquisaTipoListaID = PIL.PesquisaTipoListaID
            JOIN      BrSupply.dbo.BR_PrecoEstoque         PE  WITH (NOLOCK) ON PE.ItemID               = I.ItemID {filtro}
            JOIN      BrSupply.dbo.BR_Estabelecimento      E   WITH (NOLOCK) ON E.EstabelecimentoID     = PE.EstabelecimentoID
            ORDER BY I.CdItem
            """;

        await using var conn   = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd    = new SqlCommand(sql, conn) { CommandTimeout = 60 };
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var ordItemID  = reader.GetOrdinal("ItemID");
        var ordCdItem  = reader.GetOrdinal("CdItem");
        var ordNmItem  = reader.GetOrdinal("NmItem");
        var ordNmEstab = reader.GetOrdinal("NmEstabelecimento");
        var ordCrit    = reader.GetOrdinal("Criticidade");
        var ordVlr     = reader.GetOrdinal("VlrCustoAquisicao");
        var ordQt      = reader.GetOrdinal("QtDispEstoque");
        var ordNmTipo  = reader.GetOrdinal("NmTipoLista");
        var ordPesqID  = reader.GetOrdinal("PesquisaTipoListaID");
        var ordPrior   = reader.GetOrdinal("Prioridade");

        var result = new List<CategorizacaoItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new CategorizacaoItem
            {
                ItemID              = reader.GetInt32(ordItemID),
                CdItem              = reader.GetString(ordCdItem),
                NmItem              = reader.GetString(ordNmItem),
                NmEstabelecimento   = reader.GetString(ordNmEstab),
                Criticidade         = reader.IsDBNull(ordCrit)   ? "" :  reader.GetString(ordCrit),
                VlrCustoAquisicao   = reader.IsDBNull(ordVlr)    ? null : reader.GetDecimal(ordVlr),
                QtDispEstoque       = reader.IsDBNull(ordQt)     ? 0   : reader.GetInt32(ordQt),
                NmTipoLista         = reader.IsDBNull(ordNmTipo) ? null : reader.GetString(ordNmTipo),
                PesquisaTipoListaID = reader.IsDBNull(ordPesqID) ? null : reader.GetInt32(ordPesqID),
                Prioridade          = reader.IsDBNull(ordPrior)  ? null : reader.GetInt32(ordPrior),
            });
        }

        cache.Set(key, (IReadOnlyList<CategorizacaoItem>)result,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        return result;
    }

    public async Task<IReadOnlyList<CategorizacaoItemSemCategoria>> GetItensSemCategoriaAsync(
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKeySemCategoria, out IReadOnlyList<CategorizacaoItemSemCategoria>? cached) && cached is not null)
            return cached;

        const string sql = """
            SELECT DISTINCT I.ItemID, I.CdItem, I.NmItem, S.NmSegmento
            FROM BrSupply.dbo.BR_Item I WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_PesquisaItemLista PIL WITH (NOLOCK) ON PIL.ItemID   = I.ItemID
            JOIN      BrSupply.dbo.BR_PrecoEstoque      E   WITH (NOLOCK) ON E.ItemID     = I.ItemID AND E.EstabelecimentoID IN (1,9)
            JOIN      BrSupply.dbo.BR_Segmento          S   WITH (NOLOCK) ON S.SegmentoID = I.SegmentoID
            WHERE I.FlagAtivo = 1 AND I.FlagCatalogo = 1
              AND NOT EXISTS (
                  SELECT 1 FROM BrSupply.dbo.BR_PrecoEstoque X WITH (NOLOCK)
                  WHERE X.ItemID = I.ItemID AND X.FlagOutlet = 1 AND X.EstabelecimentoID IN (1,9)
              )
              AND I.ItemID NOT IN (46639,46440,46441)
              AND PIL.PesquisaTipoListaID IS NULL
              AND S.FlagPadraoBrSupply = 1
            ORDER BY I.CdItem
            """;

        await using var conn   = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd    = new SqlCommand(sql, conn) { CommandTimeout = 60 };
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var ordItemID    = reader.GetOrdinal("ItemID");
        var ordCdItem    = reader.GetOrdinal("CdItem");
        var ordNmItem    = reader.GetOrdinal("NmItem");
        var ordNmSegmento = reader.GetOrdinal("NmSegmento");

        var result = new List<CategorizacaoItemSemCategoria>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new CategorizacaoItemSemCategoria
            {
                ItemID     = reader.GetInt32(ordItemID),
                CdItem     = reader.GetString(ordCdItem),
                NmItem     = reader.GetString(ordNmItem),
                NmSegmento = reader.IsDBNull(ordNmSegmento) ? null : reader.GetString(ordNmSegmento),
            });
        }

        cache.Set(CacheKeySemCategoria, (IReadOnlyList<CategorizacaoItemSemCategoria>)result,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        return result;
    }

    public async Task<IReadOnlyList<CategorizacaoTipoLista>> GetCategoriasAsync(
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKeyCategorias, out IReadOnlyList<CategorizacaoTipoLista>? cached) && cached is not null)
            return cached;

        const string sql = """
            SELECT PesquisaTipoListaID, NmTipoLista
            FROM BrSupply.dbo.BR_PesquisaTipoLista WITH (NOLOCK)
            WHERE FlagAtivo = 1
            ORDER BY NmTipoLista
            """;

        await using var conn   = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd    = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new List<CategorizacaoTipoLista>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new CategorizacaoTipoLista
            {
                PesquisaTipoListaID = reader.GetInt32(0),
                NmTipoLista         = reader.GetString(1),
            });
        }

        cache.Set(CacheKeyCategorias, (IReadOnlyList<CategorizacaoTipoLista>)result,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });

        return result;
    }

    public async Task<bool> SalvarCategoriaAsync(
        int itemId, int pesquisaTipoListaId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM BrSupply.dbo.BR_PesquisaItemLista WHERE ItemID = @ItemID)
                UPDATE BrSupply.dbo.BR_PesquisaItemLista
                   SET PesquisaTipoListaID = @PesquisaTipoListaID
                 WHERE ItemID = @ItemID
            ELSE
                INSERT INTO BrSupply.dbo.BR_PesquisaItemLista (ItemID, PesquisaTipoListaID, Prioridade)
                VALUES (@ItemID, @PesquisaTipoListaID, 0)
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ItemID", itemId);
        cmd.Parameters.AddWithValue("@PesquisaTipoListaID", pesquisaTipoListaId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

        cache.Remove(CacheKeyItens(null));
        cache.Remove(CacheKeySemCategoria);

        return rows > 0;
    }

    public async Task<bool> RemoverCategoriaAsync(int itemId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM BrSupply.dbo.BR_PesquisaItemLista WHERE ItemID = @ItemID";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ItemID", itemId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

        cache.Remove(CacheKeyItens(null));
        cache.Remove(CacheKeySemCategoria);

        return rows > 0;
    }
}
