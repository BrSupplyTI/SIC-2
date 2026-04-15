using SIC.Api.Contracts.Projetos;

namespace SIC.Api.Services;

public interface IProjetoService
{
    // ── Leitura ──────────────────────────────────────────────

    Task<ProjetoListResultDto> ListarProjetosAsync(ProjetoFilterDto filter, CancellationToken cancellationToken = default);

    Task<ProjetoComTarefasResultDto> ListarProjetosComTarefasAsync(ProjetoFilterDto filter, CancellationToken cancellationToken = default);

    Task<ProjetoDetailDto?> ObterDetalhesAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoTarefaDto>> ListarTarefasAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoParticipanteDto>> ListarParticipantesAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoHistoricoDto>> ListarHistoricoAsync(int projetoId, CancellationToken cancellationToken = default);

    // ── Lookups ──────────────────────────────────────────────

    Task<IReadOnlyList<ProjetoStatusDto>> ObterStatusListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoTarefaStatusDto>> ObterTarefaStatusListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjetoTarefaPrioridadeDto>> ObterPrioridadeListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioBuscaDto>> BuscarUsuariosAsync(string texto, CancellationToken cancellationToken = default);

    Task<bool> VerificarParticipanteAsync(int projetoId, int usuarioId, CancellationToken cancellationToken = default);

    // ── Escrita — Projeto ────────────────────────────────────

    Task<int> CriarProjetoAsync(ProjetoCriarDto dto, CancellationToken cancellationToken = default);

    Task<int> AtualizarProjetoAsync(ProjetoAtualizarDto dto, CancellationToken cancellationToken = default);

    // ── Escrita — Tarefa ─────────────────────────────────────

    Task<int> CriarTarefaAsync(TarefaCriarDto dto, CancellationToken cancellationToken = default);

    Task<int> AtualizarTarefaAsync(TarefaAtualizarDto dto, CancellationToken cancellationToken = default);

    Task<int> ExcluirTarefaAsync(int projetoTarefaId, int usuarioId, CancellationToken cancellationToken = default);

    // ── Escrita — Participante ───────────────────────────────

    Task<int> AdicionarParticipanteAsync(ParticipanteAdicionarDto dto, CancellationToken cancellationToken = default);

    Task<int> AtualizarPapelParticipanteAsync(ParticipanteAtualizarPapelDto dto, CancellationToken cancellationToken = default);

    Task<int> RemoverParticipanteAsync(int projetoParticipanteId, int usuarioLogadoId, CancellationToken cancellationToken = default);
}
