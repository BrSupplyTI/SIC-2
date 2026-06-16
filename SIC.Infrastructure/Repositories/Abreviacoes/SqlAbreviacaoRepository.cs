using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.Abreviacoes;
using SIC.Domain.Entities.Abreviacoes;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SIC.Infrastructure.Repositories.Abreviacoes;

public sealed class SqlAbreviacaoRepository(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IAbreviacaoRepository
{
    private const string VortigoUrl = "https://brsupply.vortigo.tech/abreviations/";

    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<AbreviacaoItem>> BuscarDadosAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT A.ID, A.Texto, A.Abreviacao,
                   U.NmUsuario,
                   CONVERT(VARCHAR(10), A.DataHora, 103) AS DataHora
            FROM BrWeb..NovosNegocios_Abreviacao A (NOLOCK)
            JOIN BrSupply.dbo.BR_Usuario U (NOLOCK) ON U.UsuarioID = A.UsuarioID
            ORDER BY A.DataHora DESC
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var lista = new List<AbreviacaoItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            lista.Add(new AbreviacaoItem
            {
                ID         = reader.GetInt32(0),
                Texto      = reader.GetString(1),
                Abreviacao = reader.GetString(2),
                NmUsuario  = reader.GetString(3),
                DataHora   = reader.GetString(4),
            });
        }
        return lista;
    }

    public async Task<bool> GravarAsync(string texto, string abreviacao, int usuarioId, CancellationToken cancellationToken = default)
    {
        // 1. Persiste no banco (ignora se já existir o par Texto+Abreviacao)
        const string sql = """
            IF NOT EXISTS (
                SELECT 1 FROM BrWeb..NovosNegocios_Abreviacao
                WHERE Texto = @Texto AND Abreviacao = @Abreviacao
            )
            BEGIN
                INSERT INTO BrWeb..NovosNegocios_Abreviacao (Texto, Abreviacao, UsuarioID, DataHora)
                VALUES (@Texto, @Abreviacao, @UsuarioID, GETDATE())
            END
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Texto",      texto);
        cmd.Parameters.AddWithValue("@Abreviacao", abreviacao);
        cmd.Parameters.AddWithValue("@UsuarioID",  usuarioId);

        var linhasAfetadas = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (linhasAfetadas == 0) return false; // duplicata — não notifica Vortigo

        // 2. Envia para API Vortigo apenas após inserção bem-sucedida
        await EnviarVortigoAsync("POST", new { abreviation = abreviacao, text = texto }, cancellationToken);
        return true;
    }

    public async Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        // 1. Busca texto/abreviacao para enviar ao Vortigo
        var item = await BuscarPorIdAsync(id, cancellationToken);
        if (item is not null)
            await EnviarVortigoAsync("DELETE", new { abreviation = item.Abreviacao, text = item.Texto }, cancellationToken);

        // 2. Remove do banco
        const string sql = "DELETE FROM BrWeb..NovosNegocios_Abreviacao WHERE ID = @ID";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private async Task<AbreviacaoItem?> BuscarPorIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT ID, Texto, Abreviacao, '', '' FROM BrWeb..NovosNegocios_Abreviacao WHERE ID = @ID";
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new AbreviacaoItem { ID = reader.GetInt32(0), Texto = reader.GetString(1), Abreviacao = reader.GetString(2) };
    }

    private async Task EnviarVortigoAsync(string method, object payload, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod(method), VortigoUrl) { Content = body };
            await client.SendAsync(request, cancellationToken);
        }
        catch { /* melhor esforço — não bloqueia a operação principal */ }
    }
}
