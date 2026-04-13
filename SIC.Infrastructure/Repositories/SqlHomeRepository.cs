using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;
using System.Data;
using DomainMonitor = SIC.Domain.Entities.Monitor;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlHomeRepository(IConfiguration configuration) : IHomeRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<Shortcut>> GetUserShortcutsAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT TOP 8 A.AtalhoID, A.Nome, A.Url, A.FlagExterna, A.Icone, ISNULL(UA.Estilo, '') AS Estilo
            FROM BR_Atalho A WITH (NOLOCK)
            JOIN BR_UsuarioAtalho UA WITH (NOLOCK) ON A.AtalhoID = UA.AtalhoID
            WHERE UA.UsuarioID = @UsuarioID
            ORDER BY UA.UsuarioAtalhoID
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<Shortcut>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new Shortcut
            {
                AtalhoID = reader.GetInt32(reader.GetOrdinal("AtalhoID")),
                Nome = ReadString(reader, "Nome"),
                Url = ReadString(reader, "Url"),
                FlagExterna = reader.GetInt32(reader.GetOrdinal("FlagExterna")),
                Icone = ReadString(reader, "Icone"),
                Estilo = ReadString(reader, "Estilo")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<Shortcut>> GetAllShortcutsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT AtalhoID, Nome, Url, FlagExterna, Icone
            FROM BR_Atalho WITH (NOLOCK)
            ORDER BY Nome
            """;

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<Shortcut>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new Shortcut
            {
                AtalhoID = reader.GetInt32(reader.GetOrdinal("AtalhoID")),
                Nome = ReadString(reader, "Nome"),
                Url = ReadString(reader, "Url"),
                FlagExterna = reader.GetInt32(reader.GetOrdinal("FlagExterna")),
                Icone = ReadString(reader, "Icone")
            });
        }

        return items;
    }

    public async Task AddUserShortcutAsync(int usuarioId, int atalhoId, string estilo, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            IF NOT EXISTS (
                SELECT 1 FROM BR_UsuarioAtalho WITH (NOLOCK)
                WHERE UsuarioID = @UsuarioID AND AtalhoID = @AtalhoID
            )
            INSERT INTO BR_UsuarioAtalho (UsuarioID, AtalhoID, Estilo) VALUES (@UsuarioID, @AtalhoID, @Estilo)
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;
        cmd.Parameters.Add("@AtalhoID", SqlDbType.Int).Value = atalhoId;
        cmd.Parameters.Add("@Estilo", SqlDbType.VarChar, 50).Value = string.IsNullOrWhiteSpace(estilo) ? (object)DBNull.Value : estilo;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            DELETE FROM BR_UsuarioAtalho
            WHERE UsuarioID = @UsuarioID AND AtalhoID = @AtalhoID
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;
        cmd.Parameters.Add("@AtalhoID", SqlDbType.Int).Value = atalhoId;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CurrencyQuote>> GetCurrencyQuotesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT Moeda, Nome, Valor, Variacao, DtUltimaAtualizacao AS DataAtualizacao
            FROM BRWeb..CotacaoesMoedas WITH (NOLOCK)
            """;

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<CurrencyQuote>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CurrencyQuote
            {
                Moeda = ReadString(reader, "Moeda"),
                Nome = ReadString(reader, "Nome"),
                Valor = reader.GetDecimal(reader.GetOrdinal("Valor")),
                Variacao = reader.GetDecimal(reader.GetOrdinal("Variacao")),
                DataAtualizacao = reader.GetDateTime(reader.GetOrdinal("DataAtualizacao"))
            });
        }

        return items;
    }

    public async Task<WeatherInfo?> GetWeatherInfoAsync(int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT C.EstabelecimentoID,
                   C.Cidade,
                   C.UF,
                   C.Temperatura,
                   C.Sensacao,
                   C.Umidade,
                   C.VelocidadeVento,
                   C.Descricao,
                   C.DtUltimaAtualizacao
            FROM BRWeb..ClimaCidades C WITH (NOLOCK)
            WHERE C.EstabelecimentoID = @EstabelecimentoID
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@EstabelecimentoID", SqlDbType.Int).Value = estabelecimentoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new WeatherInfo
        {
            EstabelecimentoID = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
            Cidade = ReadString(reader, "Cidade"),
            UF = ReadString(reader, "UF"),
            Temperatura = reader.GetDecimal(reader.GetOrdinal("Temperatura")),
            Sensacao = reader.GetDecimal(reader.GetOrdinal("Sensacao")),
            Umidade = reader.GetInt32(reader.GetOrdinal("Umidade")),
            VelocidadeVento = reader.GetDecimal(reader.GetOrdinal("VelocidadeVento")),
            Descricao = ReadString(reader, "Descricao"),
            DtUltimaAtualizacao = reader.GetDateTime(reader.GetOrdinal("DtUltimaAtualizacao"))
        };
    }

    public async Task<IReadOnlyList<DomainMonitor>> GetAllMonitorsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT MonitorID, Nivel, Icone, Nome, Titulo, PromptValor
            FROM BR_Monitor WITH (NOLOCK)
            ORDER BY Nivel, Nome
            """;

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<DomainMonitor>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new DomainMonitor
            {
                MonitorID = reader.GetInt32(reader.GetOrdinal("MonitorID")),
                Nivel = ReadString(reader, "Nivel"),
                Icone = ReadString(reader, "Icone"),
                Nome = ReadString(reader, "Nome"),
                Titulo = ReadString(reader, "Titulo"),
                PromptValor = ReadString(reader, "PromptValor")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<UserMonitorResult>> GetUserMonitorResultsAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT UM.UsuarioMonitorID, M.MonitorID, M.Nivel, M.Icone, M.Nome, M.Titulo,
                   M.ComandoSQL, UM.Valor
            FROM BR_UsuarioMonitor UM WITH (NOLOCK)
            JOIN BR_Monitor M WITH (NOLOCK) ON M.MonitorID = UM.MonitorID
            WHERE UM.UsuarioID = @UsuarioID
            ORDER BY M.Nivel, M.Nome
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var pendentes = new List<(int UsuarioMonitorID, int MonitorID, string Nivel, string Icone, string Nome, string Titulo, string ComandoSQL, string Valor)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            pendentes.Add((
                reader.GetInt32(reader.GetOrdinal("UsuarioMonitorID")),
                reader.GetInt32(reader.GetOrdinal("MonitorID")),
                ReadString(reader, "Nivel"),
                ReadString(reader, "Icone"),
                ReadString(reader, "Nome"),
                ReadString(reader, "Titulo"),
                ReadString(reader, "ComandoSQL"),
                ReadString(reader, "Valor")
            ));
        }

        await reader.CloseAsync();

        var items = new List<UserMonitorResult>();

        foreach (var p in pendentes)
        {
            var resultado = "Sem resultado";
            try
            {
                var sqlExec = p.ComandoSQL.Replace("{valor}", p.Valor);
                await using var cmdExec = new SqlCommand(sqlExec, connection);
                cmdExec.CommandTimeout = 10;
                var scalar = await cmdExec.ExecuteScalarAsync(cancellationToken);
                if (scalar is not null and not DBNull)
                    resultado = scalar.ToString()!.Trim();
            }
            catch
            {
                resultado = "Erro ao consultar";
            }

            items.Add(new UserMonitorResult
            {
                UsuarioMonitorID = p.UsuarioMonitorID,
                MonitorID = p.MonitorID,
                Nivel = p.Nivel,
                Icone = p.Icone,
                Nome = p.Nome,
                Titulo = p.Titulo,
                Resultado = resultado
            });
        }

        return items;
    }

    public async Task AddUserMonitorAsync(int usuarioId, int monitorId, string valor, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO BR_UsuarioMonitor (UsuarioID, MonitorID, DataHora, Valor)
            VALUES (@UsuarioID, @MonitorID, GETDATE(), @Valor)
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;
        cmd.Parameters.Add("@MonitorID", SqlDbType.Int).Value = monitorId;
        cmd.Parameters.Add("@Valor", SqlDbType.VarChar, 500).Value = valor;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveUserMonitorAsync(int usuarioId, int usuarioMonitorId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            DELETE FROM BR_UsuarioMonitor
            WHERE UsuarioMonitorID = @UsuarioMonitorID AND UsuarioID = @UsuarioID
            """;

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@UsuarioMonitorID", SqlDbType.Int).Value = usuarioMonitorId;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();
    }
}
