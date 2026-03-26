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
}
