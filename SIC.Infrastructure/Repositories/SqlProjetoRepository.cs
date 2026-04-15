using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;
using System.Data;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlProjetoRepository(IConfiguration configuration) : IProjetoRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    // ── Leitura ──────────────────────────────────────────────

    public async Task<IReadOnlyList<ProjetoListItem>> ListarProjetosAsync(
        int pageNumber,
        int pageSize,
        string texto,
        int projetoStatusId,
        string orderBy,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetosListar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
        cmd.Parameters.Add("@Texto", SqlDbType.VarChar, 200).Value = texto;
        cmd.Parameters.Add("@ProjetoStatusID", SqlDbType.Int).Value = projetoStatusId;
        cmd.Parameters.Add("@OrderBy", SqlDbType.VarChar, 50).Value = orderBy;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ProjetoListItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjetoListItem
            {
                ProjetoID = reader.GetInt32(reader.GetOrdinal("ProjetoID")),
                NmProjeto = ReadString(reader, "NmProjeto"),
                DsProjeto = ReadString(reader, "DsProjeto"),
                ProjetoStatusID = reader.GetInt32(reader.GetOrdinal("ProjetoStatusID")),
                NmStatus = ReadString(reader, "NmStatus"),
                CdCorStatus = ReadString(reader, "CdCorStatus"),
                DtInicio = ReadNullableDateTime(reader, "DtInicio"),
                DtPrevisaoFim = ReadNullableDateTime(reader, "DtPrevisaoFim"),
                DtFimReal = ReadNullableDateTime(reader, "DtFimReal"),
                UsuarioCriadorID = reader.GetInt32(reader.GetOrdinal("UsuarioCriadorID")),
                NmCriador = ReadString(reader, "NmCriador"),
                DtCriacao = reader.GetDateTime(reader.GetOrdinal("DtCriacao")),
                QtTarefas = reader.GetInt32(reader.GetOrdinal("QtTarefas")),
                QtTarefasConcluidas = reader.GetInt32(reader.GetOrdinal("QtTarefasConcluidas")),
                QtParticipantes = reader.GetInt32(reader.GetOrdinal("QtParticipantes")),
                TotalRegistros = reader.GetInt32(reader.GetOrdinal("TotalRegistros"))
            });
        }

        return items;
    }

    public async Task<ProjetoDetail?> ObterDetalhesAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoDetalhes", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new ProjetoDetail
        {
            ProjetoID = reader.GetInt32(reader.GetOrdinal("ProjetoID")),
            NmProjeto = ReadString(reader, "NmProjeto"),
            DsProjeto = ReadString(reader, "DsProjeto"),
            ProjetoStatusID = reader.GetInt32(reader.GetOrdinal("ProjetoStatusID")),
            NmStatus = ReadString(reader, "NmStatus"),
            CdCorStatus = ReadString(reader, "CdCorStatus"),
            DtInicio = ReadNullableDateTime(reader, "DtInicio"),
            DtPrevisaoFim = ReadNullableDateTime(reader, "DtPrevisaoFim"),
            DtFimReal = ReadNullableDateTime(reader, "DtFimReal"),
            UsuarioCriadorID = reader.GetInt32(reader.GetOrdinal("UsuarioCriadorID")),
            NmCriador = ReadString(reader, "NmCriador"),
            DtCriacao = reader.GetDateTime(reader.GetOrdinal("DtCriacao")),
            DtUltimaAtualizacao = ReadNullableDateTime(reader, "DtUltimaAtualizacao"),
            QtTarefas = reader.GetInt32(reader.GetOrdinal("QtTarefas")),
            QtTarefasConcluidas = reader.GetInt32(reader.GetOrdinal("QtTarefasConcluidas"))
        };
    }

    public async Task<IReadOnlyList<ProjetoTarefa>> ListarTarefasAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoTarefasListar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ProjetoTarefa>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjetoTarefa
            {
                ProjetoTarefaID = reader.GetInt32(reader.GetOrdinal("ProjetoTarefaID")),
                ProjetoID = reader.GetInt32(reader.GetOrdinal("ProjetoID")),
                NmTarefa = ReadString(reader, "NmTarefa"),
                DsTarefa = ReadNullableString(reader, "DsTarefa"),
                ProjetoTarefaStatusID = reader.GetInt32(reader.GetOrdinal("ProjetoTarefaStatusID")),
                NmStatus = ReadString(reader, "NmStatus"),
                CdCorStatus = ReadString(reader, "CdCorStatus"),
                ProjetoTarefaPrioridadeID = reader.GetInt32(reader.GetOrdinal("ProjetoTarefaPrioridadeID")),
                NmPrioridade = ReadString(reader, "NmPrioridade"),
                CdCorPrioridade = ReadString(reader, "CdCorPrioridade"),
                UsuarioResponsavelID = ReadNullableInt32(reader, "UsuarioResponsavelID"),
                NmResponsavel = ReadString(reader, "NmResponsavel"),
                DtInicio = ReadNullableDateTime(reader, "DtInicio"),
                DtPrevisaoFim = ReadNullableDateTime(reader, "DtPrevisaoFim"),
                DtFimReal = ReadNullableDateTime(reader, "DtFimReal"),
                NrOrdem = reader.GetInt32(reader.GetOrdinal("NrOrdem")),
                ProjetoTarefaPaiID = ReadNullableInt32(reader, "ProjetoTarefaPaiID")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProjetoParticipante>> ListarParticipantesAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoParticipantesListar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ProjetoParticipante>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjetoParticipante
            {
                ProjetoParticipanteID = reader.GetInt32(reader.GetOrdinal("ProjetoParticipanteID")),
                UsuarioID = reader.GetInt32(reader.GetOrdinal("UsuarioID")),
                NmUsuario = ReadString(reader, "NmUsuario"),
                NmPapel = ReadString(reader, "NmPapel"),
                DtEntrada = reader.GetDateTime(reader.GetOrdinal("DtEntrada"))
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProjetoHistorico>> ListarHistoricoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoHistoricoListar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ProjetoHistorico>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjetoHistorico
            {
                ProjetoHistoricoID = reader.GetInt32(reader.GetOrdinal("ProjetoHistoricoID")),
                UsuarioID = reader.GetInt32(reader.GetOrdinal("UsuarioID")),
                NmUsuario = ReadString(reader, "NmUsuario"),
                DsAcao = ReadString(reader, "DsAcao"),
                DtAcao = reader.GetDateTime(reader.GetOrdinal("DtAcao"))
            });
        }

        return items;
    }

    // ── Lookups ──────────────────────────────────────────────

    public async Task<IReadOnlyList<ProjetoStatus>> ObterStatusListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ProjetoStatusID, NmStatus, CdCor, NrOrdem
            FROM BrWeb.dbo.BR_ProjetoStatus WITH (NOLOCK)
            WHERE FlagAtivo = 1
            ORDER BY NrOrdem
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ProjetoStatus>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjetoStatus
            {
                ProjetoStatusID = reader.GetInt32(reader.GetOrdinal("ProjetoStatusID")),
                NmStatus = ReadString(reader, "NmStatus"),
                CdCor = ReadString(reader, "CdCor"),
                NrOrdem = reader.GetInt32(reader.GetOrdinal("NrOrdem"))
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProjetoTarefaStatus>> ObterTarefaStatusListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ProjetoTarefaStatusID, NmStatus, CdCor, NrOrdem
            FROM BrWeb.dbo.BR_ProjetoTarefaStatus WITH (NOLOCK)
            WHERE FlagAtivo = 1
            ORDER BY NrOrdem
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ProjetoTarefaStatus>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjetoTarefaStatus
            {
                ProjetoTarefaStatusID = reader.GetInt32(reader.GetOrdinal("ProjetoTarefaStatusID")),
                NmStatus = ReadString(reader, "NmStatus"),
                CdCor = ReadString(reader, "CdCor"),
                NrOrdem = reader.GetInt32(reader.GetOrdinal("NrOrdem"))
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ProjetoTarefaPrioridade>> ObterPrioridadeListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ProjetoTarefaPrioridadeID, NmPrioridade, CdCor, NrOrdem
            FROM BrWeb.dbo.BR_ProjetoTarefaPrioridade WITH (NOLOCK)
            WHERE FlagAtivo = 1
            ORDER BY NrOrdem
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ProjetoTarefaPrioridade>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProjetoTarefaPrioridade
            {
                ProjetoTarefaPrioridadeID = reader.GetInt32(reader.GetOrdinal("ProjetoTarefaPrioridadeID")),
                NmPrioridade = ReadString(reader, "NmPrioridade"),
                CdCor = ReadString(reader, "CdCor"),
                NrOrdem = reader.GetInt32(reader.GetOrdinal("NrOrdem"))
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<UsuarioBusca>> BuscarUsuariosAsync(string texto, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT U.UsuarioID, U.NmUsuario
            FROM BrSupply..BR_Usuario U (NOLOCK)
            WHERE U.FlagAtivo = 1
              AND U.UsuarioID NOT IN (1,2,551,553,661,685,769,882,1158,1405,1670,1671,1672,1725,1916,2019,2278,2539,2582,2596)
              AND (@Texto = '' OR U.NmUsuario LIKE '%' + @Texto + '%')
            ORDER BY U.NmUsuario
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@Texto", SqlDbType.VarChar, 200).Value = texto ?? "";

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<UsuarioBusca>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new UsuarioBusca
            {
                UsuarioID = reader.GetInt32(reader.GetOrdinal("UsuarioID")),
                NmUsuario = ReadString(reader, "NmUsuario")
            });
        }

        return items;
    }

    public async Task<bool> VerificarParticipanteAsync(int projetoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoVerificarParticipante", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            return reader.GetBoolean(reader.GetOrdinal("EhParticipante"));

        return false;
    }

    // ── Escrita — Projeto ────────────────────────────────────

    public async Task<int> CriarProjetoAsync(
        string nmProjeto,
        string dsProjeto,
        int projetoStatusId,
        DateTime? dtInicio,
        DateTime? dtPrevisaoFim,
        int usuarioCriadorId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoCriar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@NmProjeto", SqlDbType.VarChar, 200).Value = nmProjeto;
        cmd.Parameters.Add("@DsProjeto", SqlDbType.VarChar, 2000).Value = dsProjeto;
        cmd.Parameters.Add("@ProjetoStatusID", SqlDbType.Int).Value = projetoStatusId;
        cmd.Parameters.Add("@DtInicio", SqlDbType.Date).Value = (object?)dtInicio ?? DBNull.Value;
        cmd.Parameters.Add("@DtPrevisaoFim", SqlDbType.Date).Value = (object?)dtPrevisaoFim ?? DBNull.Value;
        cmd.Parameters.Add("@UsuarioCriadorID", SqlDbType.Int).Value = usuarioCriadorId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoID"));
    }

    public async Task<int> AtualizarProjetoAsync(
        int projetoId,
        string nmProjeto,
        string dsProjeto,
        int projetoStatusId,
        DateTime? dtInicio,
        DateTime? dtPrevisaoFim,
        DateTime? dtFimReal,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoAtualizar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;
        cmd.Parameters.Add("@NmProjeto", SqlDbType.VarChar, 200).Value = nmProjeto;
        cmd.Parameters.Add("@DsProjeto", SqlDbType.VarChar, 2000).Value = dsProjeto;
        cmd.Parameters.Add("@ProjetoStatusID", SqlDbType.Int).Value = projetoStatusId;
        cmd.Parameters.Add("@DtInicio", SqlDbType.Date).Value = (object?)dtInicio ?? DBNull.Value;
        cmd.Parameters.Add("@DtPrevisaoFim", SqlDbType.Date).Value = (object?)dtPrevisaoFim ?? DBNull.Value;
        cmd.Parameters.Add("@DtFimReal", SqlDbType.Date).Value = (object?)dtFimReal ?? DBNull.Value;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoID"));
    }

    // ── Escrita — Tarefa ─────────────────────────────────────

    public async Task<int> CriarTarefaAsync(
        int projetoId,
        string nmTarefa,
        string? dsTarefa,
        int projetoTarefaStatusId,
        int projetoTarefaPrioridadeId,
        int? usuarioResponsavelId,
        DateTime? dtInicio,
        DateTime? dtPrevisaoFim,
        int? projetoTarefaPaiId,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoTarefaCriar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;
        cmd.Parameters.Add("@NmTarefa", SqlDbType.VarChar, 300).Value = nmTarefa;
        cmd.Parameters.Add("@DsTarefa", SqlDbType.VarChar, 2000).Value = (object?)dsTarefa ?? DBNull.Value;
        cmd.Parameters.Add("@ProjetoTarefaStatusID", SqlDbType.Int).Value = projetoTarefaStatusId;
        cmd.Parameters.Add("@ProjetoTarefaPrioridadeID", SqlDbType.Int).Value = projetoTarefaPrioridadeId;
        cmd.Parameters.Add("@UsuarioResponsavelID", SqlDbType.Int).Value = (object?)usuarioResponsavelId ?? DBNull.Value;
        cmd.Parameters.Add("@DtInicio", SqlDbType.Date).Value = (object?)dtInicio ?? DBNull.Value;
        cmd.Parameters.Add("@DtPrevisaoFim", SqlDbType.Date).Value = (object?)dtPrevisaoFim ?? DBNull.Value;
        cmd.Parameters.Add("@ProjetoTarefaPaiID", SqlDbType.Int).Value = (object?)projetoTarefaPaiId ?? DBNull.Value;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoTarefaID"));
    }

    public async Task<int> AtualizarTarefaAsync(
        int projetoTarefaId,
        string nmTarefa,
        string? dsTarefa,
        int projetoTarefaStatusId,
        int projetoTarefaPrioridadeId,
        int? usuarioResponsavelId,
        DateTime? dtInicio,
        DateTime? dtPrevisaoFim,
        DateTime? dtFimReal,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoTarefaAtualizar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoTarefaID", SqlDbType.Int).Value = projetoTarefaId;
        cmd.Parameters.Add("@NmTarefa", SqlDbType.VarChar, 300).Value = nmTarefa;
        cmd.Parameters.Add("@DsTarefa", SqlDbType.VarChar, 2000).Value = (object?)dsTarefa ?? DBNull.Value;
        cmd.Parameters.Add("@ProjetoTarefaStatusID", SqlDbType.Int).Value = projetoTarefaStatusId;
        cmd.Parameters.Add("@ProjetoTarefaPrioridadeID", SqlDbType.Int).Value = projetoTarefaPrioridadeId;
        cmd.Parameters.Add("@UsuarioResponsavelID", SqlDbType.Int).Value = (object?)usuarioResponsavelId ?? DBNull.Value;
        cmd.Parameters.Add("@DtInicio", SqlDbType.Date).Value = (object?)dtInicio ?? DBNull.Value;
        cmd.Parameters.Add("@DtPrevisaoFim", SqlDbType.Date).Value = (object?)dtPrevisaoFim ?? DBNull.Value;
        cmd.Parameters.Add("@DtFimReal", SqlDbType.Date).Value = (object?)dtFimReal ?? DBNull.Value;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoTarefaID"));
    }

    public async Task<int> ExcluirTarefaAsync(
        int projetoTarefaId,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoTarefaExcluir", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoTarefaID", SqlDbType.Int).Value = projetoTarefaId;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoTarefaID"));
    }

    // ── Escrita — Participante ───────────────────────────────

    public async Task<int> AdicionarParticipanteAsync(
        int projetoId,
        int usuarioId,
        string nmPapel,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoParticipanteAdicionar", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoID", SqlDbType.Int).Value = projetoId;
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;
        cmd.Parameters.Add("@NmPapel", SqlDbType.VarChar, 100).Value = nmPapel;
        cmd.Parameters.Add("@UsuarioLogadoID", SqlDbType.Int).Value = usuarioLogadoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoParticipanteID"));
    }

    public async Task<int> AtualizarPapelParticipanteAsync(
        int projetoParticipanteId,
        string nmPapel,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoParticipanteAtualizarPapel", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoParticipanteID", SqlDbType.Int).Value = projetoParticipanteId;
        cmd.Parameters.Add("@NmPapel", SqlDbType.VarChar, 100).Value = nmPapel;
        cmd.Parameters.Add("@UsuarioLogadoID", SqlDbType.Int).Value = usuarioLogadoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoParticipanteID"));
    }

    public async Task<int> RemoverParticipanteAsync(
        int projetoParticipanteId,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("BrWeb.dbo.SIC_ProjetoParticipanteRemover", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ProjetoParticipanteID", SqlDbType.Int).Value = projetoParticipanteId;
        cmd.Parameters.Add("@UsuarioLogadoID", SqlDbType.Int).Value = usuarioLogadoId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return reader.GetInt32(reader.GetOrdinal("ProjetoParticipanteID"));
    }

    // ── Helpers ──────────────────────────────────────────────

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
