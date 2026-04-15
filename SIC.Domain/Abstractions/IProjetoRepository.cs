namespace SIC.Domain.Abstractions;

using SIC.Domain.Entities;

public interface IProjetoRepository
{
    // ── Leitura ──────────────────────────────────────────────

    Task<IReadOnlyList<ProjetoListItem>> ListarProjetosAsync(
        int pageNumber,
        int pageSize,
        string texto,
        int projetoStatusId,
        string orderBy,
        CancellationToken cancellationToken = default);

    Task<ProjetoDetail?> ObterDetalhesAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoTarefa>> ListarTarefasAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoParticipante>> ListarParticipantesAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoHistorico>> ListarHistoricoAsync(int projetoId, CancellationToken cancellationToken = default);

    // ── Lookups ──────────────────────────────────────────────

    Task<IReadOnlyList<ProjetoStatus>> ObterStatusListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoTarefaStatus>> ObterTarefaStatusListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoTarefaPrioridade>> ObterPrioridadeListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioBusca>> BuscarUsuariosAsync(string texto, CancellationToken cancellationToken = default);

    Task<bool> VerificarParticipanteAsync(int projetoId, int usuarioId, CancellationToken cancellationToken = default);

    // ── Escrita — Projeto ────────────────────────────────────

    Task<int> CriarProjetoAsync(
        string nmProjeto,
        string dsProjeto,
        int projetoStatusId,
        DateTime? dtInicio,
        DateTime? dtPrevisaoFim,
        int usuarioCriadorId,
        CancellationToken cancellationToken = default);

    Task<int> AtualizarProjetoAsync(
        int projetoId,
        string nmProjeto,
        string dsProjeto,
        int projetoStatusId,
        DateTime? dtInicio,
        DateTime? dtPrevisaoFim,
        DateTime? dtFimReal,
        int usuarioId,
        CancellationToken cancellationToken = default);

    // ── Escrita — Tarefa ─────────────────────────────────────

    Task<int> CriarTarefaAsync(
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
        CancellationToken cancellationToken = default);

    Task<int> AtualizarTarefaAsync(
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
        CancellationToken cancellationToken = default);

    Task<int> ExcluirTarefaAsync(
        int projetoTarefaId,
        int usuarioId,
        CancellationToken cancellationToken = default);

    // ── Escrita — Participante ───────────────────────────────

    Task<int> AdicionarParticipanteAsync(
        int projetoId,
        int usuarioId,
        string nmPapel,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default);

    Task<int> AtualizarPapelParticipanteAsync(
        int projetoParticipanteId,
        string nmPapel,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default);

    Task<int> RemoverParticipanteAsync(
        int projetoParticipanteId,
        int usuarioLogadoId,
        CancellationToken cancellationToken = default);
}
