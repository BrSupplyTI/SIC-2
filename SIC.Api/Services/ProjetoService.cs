using SIC.Api.Contracts.Projetos;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class ProjetoService(IProjetoRepository repository) : IProjetoService
{
    private const string DateFormat = "dd/MM/yyyy";

    // ── Leitura ──────────────────────────────────────────────

    public async Task<ProjetoListResultDto> ListarProjetosAsync(ProjetoFilterDto filter, CancellationToken cancellationToken = default)
    {
        var items = await repository.ListarProjetosAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.Texto ?? string.Empty,
            filter.ProjetoStatusID,
            filter.OrderBy ?? "Recentes",
            cancellationToken);

        var totalRegistros = items.Count > 0 ? items[0].TotalRegistros : 0;

        var dtos = items.Select(i => new ProjetoListItemDto
        {
            ProjetoID = i.ProjetoID,
            NmProjeto = i.NmProjeto,
            DsProjeto = i.DsProjeto,
            ProjetoStatusID = i.ProjetoStatusID,
            NmStatus = i.NmStatus,
            CdCorStatus = i.CdCorStatus,
            DtInicio = i.DtInicio?.ToString(DateFormat),
            DtPrevisaoFim = i.DtPrevisaoFim?.ToString(DateFormat),
            DtFimReal = i.DtFimReal?.ToString(DateFormat),
            UsuarioCriadorID = i.UsuarioCriadorID,
            NmCriador = i.NmCriador,
            DtCriacao = i.DtCriacao.ToString(DateFormat),
            QtTarefas = i.QtTarefas,
            QtTarefasConcluidas = i.QtTarefasConcluidas,
            QtParticipantes = i.QtParticipantes
        }).ToList();

        return new ProjetoListResultDto
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filter.PageSize) : 0,
            Itens = dtos
        };
    }

    public async Task<ProjetoComTarefasResultDto> ListarProjetosComTarefasAsync(ProjetoFilterDto filter, CancellationToken cancellationToken = default)
    {
        var items = await repository.ListarProjetosAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.Texto ?? string.Empty,
            filter.ProjetoStatusID,
            filter.OrderBy ?? "Recentes",
            cancellationToken);

        var totalRegistros = items.Count > 0 ? items[0].TotalRegistros : 0;

        var tarefasTasks = items.Select(i => repository.ListarTarefasAsync(i.ProjetoID, cancellationToken)).ToArray();
        var tarefasResults = await Task.WhenAll(tarefasTasks);

        var projetosComTarefas = new List<ProjetoComTarefasItemDto>(items.Count);

        for (var idx = 0; idx < items.Count; idx++)
        {
            var i = items[idx];
            var tarefas = tarefasResults[idx];

            var tarefaDtos = tarefas.Select(t => new ProjetoTarefaDto
            {
                ProjetoTarefaID = t.ProjetoTarefaID,
                ProjetoID = t.ProjetoID,
                NmTarefa = t.NmTarefa,
                DsTarefa = t.DsTarefa,
                ProjetoTarefaStatusID = t.ProjetoTarefaStatusID,
                NmStatus = t.NmStatus,
                CdCorStatus = t.CdCorStatus,
                ProjetoTarefaPrioridadeID = t.ProjetoTarefaPrioridadeID,
                NmPrioridade = t.NmPrioridade,
                CdCorPrioridade = t.CdCorPrioridade,
                UsuarioResponsavelID = t.UsuarioResponsavelID,
                NmResponsavel = t.NmResponsavel,
                DtInicio = t.DtInicio?.ToString(DateFormat),
                DtPrevisaoFim = t.DtPrevisaoFim?.ToString(DateFormat),
                DtFimReal = t.DtFimReal?.ToString(DateFormat),
                NrOrdem = t.NrOrdem,
                ProjetoTarefaPaiID = t.ProjetoTarefaPaiID
            }).ToList();

            projetosComTarefas.Add(new ProjetoComTarefasItemDto
            {
                ProjetoID = i.ProjetoID,
                NmProjeto = i.NmProjeto,
                DsProjeto = i.DsProjeto,
                ProjetoStatusID = i.ProjetoStatusID,
                NmStatus = i.NmStatus,
                CdCorStatus = i.CdCorStatus,
                DtInicio = i.DtInicio?.ToString(DateFormat),
                DtPrevisaoFim = i.DtPrevisaoFim?.ToString(DateFormat),
                DtFimReal = i.DtFimReal?.ToString(DateFormat),
                UsuarioCriadorID = i.UsuarioCriadorID,
                NmCriador = i.NmCriador,
                DtCriacao = i.DtCriacao.ToString(DateFormat),
                QtTarefas = i.QtTarefas,
                QtTarefasConcluidas = i.QtTarefasConcluidas,
                QtParticipantes = i.QtParticipantes,
                Tarefas = tarefaDtos
            });
        }

        return new ProjetoComTarefasResultDto
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filter.PageSize) : 0,
            Itens = projetosComTarefas
        };
    }

    public async Task<ProjetoDetailDto?> ObterDetalhesAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var entity = await repository.ObterDetalhesAsync(projetoId, cancellationToken);
        if (entity is null) return null;

        return new ProjetoDetailDto
        {
            ProjetoID = entity.ProjetoID,
            NmProjeto = entity.NmProjeto,
            DsProjeto = entity.DsProjeto,
            ProjetoStatusID = entity.ProjetoStatusID,
            NmStatus = entity.NmStatus,
            CdCorStatus = entity.CdCorStatus,
            DtInicio = entity.DtInicio?.ToString(DateFormat),
            DtPrevisaoFim = entity.DtPrevisaoFim?.ToString(DateFormat),
            DtFimReal = entity.DtFimReal?.ToString(DateFormat),
            UsuarioCriadorID = entity.UsuarioCriadorID,
            NmCriador = entity.NmCriador,
            DtCriacao = entity.DtCriacao.ToString(DateFormat),
            DtUltimaAtualizacao = entity.DtUltimaAtualizacao?.ToString(DateFormat),
            QtTarefas = entity.QtTarefas,
            QtTarefasConcluidas = entity.QtTarefasConcluidas
        };
    }

    public async Task<IReadOnlyList<ProjetoTarefaDto>> ListarTarefasAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.ListarTarefasAsync(projetoId, cancellationToken);

        return items.Select(t => new ProjetoTarefaDto
        {
            ProjetoTarefaID = t.ProjetoTarefaID,
            ProjetoID = t.ProjetoID,
            NmTarefa = t.NmTarefa,
            DsTarefa = t.DsTarefa,
            ProjetoTarefaStatusID = t.ProjetoTarefaStatusID,
            NmStatus = t.NmStatus,
            CdCorStatus = t.CdCorStatus,
            ProjetoTarefaPrioridadeID = t.ProjetoTarefaPrioridadeID,
            NmPrioridade = t.NmPrioridade,
            CdCorPrioridade = t.CdCorPrioridade,
            UsuarioResponsavelID = t.UsuarioResponsavelID,
            NmResponsavel = t.NmResponsavel,
            DtInicio = t.DtInicio?.ToString(DateFormat),
            DtPrevisaoFim = t.DtPrevisaoFim?.ToString(DateFormat),
            DtFimReal = t.DtFimReal?.ToString(DateFormat),
            NrOrdem = t.NrOrdem,
            ProjetoTarefaPaiID = t.ProjetoTarefaPaiID
        }).ToList();
    }

    public async Task<IReadOnlyList<ProjetoParticipanteDto>> ListarParticipantesAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.ListarParticipantesAsync(projetoId, cancellationToken);

        return items.Select(p => new ProjetoParticipanteDto
        {
            ProjetoParticipanteID = p.ProjetoParticipanteID,
            UsuarioID = p.UsuarioID,
            NmUsuario = p.NmUsuario,
            NmPapel = p.NmPapel,
            DtEntrada = p.DtEntrada.ToString(DateFormat)
        }).ToList();
    }

    public async Task<IReadOnlyList<ProjetoHistoricoDto>> ListarHistoricoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.ListarHistoricoAsync(projetoId, cancellationToken);

        return items.Select(h => new ProjetoHistoricoDto
        {
            ProjetoHistoricoID = h.ProjetoHistoricoID,
            UsuarioID = h.UsuarioID,
            NmUsuario = h.NmUsuario,
            DsAcao = h.DsAcao,
            DtAcao = h.DtAcao.ToString("dd/MM/yyyy HH:mm")
        }).ToList();
    }

    // ── Lookups ──────────────────────────────────────────────

    public async Task<IReadOnlyList<ProjetoStatusDto>> ObterStatusListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.ObterStatusListAsync(cancellationToken);

        return items.Select(s => new ProjetoStatusDto
        {
            ProjetoStatusID = s.ProjetoStatusID,
            NmStatus = s.NmStatus,
            CdCor = s.CdCor
        }).ToList();
    }

    public async Task<IReadOnlyList<ProjetoTarefaStatusDto>> ObterTarefaStatusListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.ObterTarefaStatusListAsync(cancellationToken);

        return items.Select(s => new ProjetoTarefaStatusDto
        {
            ProjetoTarefaStatusID = s.ProjetoTarefaStatusID,
            NmStatus = s.NmStatus,
            CdCor = s.CdCor,
            NrOrdem = s.NrOrdem
        }).ToList();
    }

    public async Task<IReadOnlyList<ProjetoTarefaPrioridadeDto>> ObterPrioridadeListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.ObterPrioridadeListAsync(cancellationToken);

        return items.Select(p => new ProjetoTarefaPrioridadeDto
        {
            ProjetoTarefaPrioridadeID = p.ProjetoTarefaPrioridadeID,
            NmPrioridade = p.NmPrioridade,
            CdCor = p.CdCor
        }).ToList();
    }

    public async Task<IReadOnlyList<UsuarioBuscaDto>> BuscarUsuariosAsync(string texto, CancellationToken cancellationToken = default)
    {
        var items = await repository.BuscarUsuariosAsync(texto, cancellationToken);

        return items.Select(u => new UsuarioBuscaDto
        {
            UsuarioID = u.UsuarioID,
            NmUsuario = u.NmUsuario
        }).ToList();
    }

    public async Task<bool> VerificarParticipanteAsync(int projetoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        return await repository.VerificarParticipanteAsync(projetoId, usuarioId, cancellationToken);
    }

    // ── Escrita — Projeto ────────────────────────────────────

    public async Task<int> CriarProjetoAsync(ProjetoCriarDto dto, CancellationToken cancellationToken = default)
    {
        return await repository.CriarProjetoAsync(
            dto.NmProjeto,
            dto.DsProjeto,
            dto.ProjetoStatusID,
            ParseDate(dto.DtInicio),
            ParseDate(dto.DtPrevisaoFim),
            dto.UsuarioCriadorID,
            cancellationToken);
    }

    public async Task<int> AtualizarProjetoAsync(ProjetoAtualizarDto dto, CancellationToken cancellationToken = default)
    {
        return await repository.AtualizarProjetoAsync(
            dto.ProjetoID,
            dto.NmProjeto,
            dto.DsProjeto,
            dto.ProjetoStatusID,
            ParseDate(dto.DtInicio),
            ParseDate(dto.DtPrevisaoFim),
            ParseDate(dto.DtFimReal),
            dto.UsuarioID,
            cancellationToken);
    }

    // ── Escrita — Tarefa ─────────────────────────────────────

    public async Task<int> CriarTarefaAsync(TarefaCriarDto dto, CancellationToken cancellationToken = default)
    {
        return await repository.CriarTarefaAsync(
            dto.ProjetoID,
            dto.NmTarefa,
            dto.DsTarefa,
            dto.ProjetoTarefaStatusID,
            dto.ProjetoTarefaPrioridadeID,
            dto.UsuarioResponsavelID,
            ParseDate(dto.DtInicio),
            ParseDate(dto.DtPrevisaoFim),
            dto.ProjetoTarefaPaiID,
            dto.UsuarioID,
            cancellationToken);
    }

    public async Task<int> AtualizarTarefaAsync(TarefaAtualizarDto dto, CancellationToken cancellationToken = default)
    {
        return await repository.AtualizarTarefaAsync(
            dto.ProjetoTarefaID,
            dto.NmTarefa,
            dto.DsTarefa,
            dto.ProjetoTarefaStatusID,
            dto.ProjetoTarefaPrioridadeID,
            dto.UsuarioResponsavelID,
            ParseDate(dto.DtInicio),
            ParseDate(dto.DtPrevisaoFim),
            ParseDate(dto.DtFimReal),
            dto.UsuarioID,
            cancellationToken);
    }

    public async Task<int> ExcluirTarefaAsync(int projetoTarefaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        return await repository.ExcluirTarefaAsync(projetoTarefaId, usuarioId, cancellationToken);
    }

    // ── Escrita — Participante ───────────────────────────────

    public async Task<int> AdicionarParticipanteAsync(ParticipanteAdicionarDto dto, CancellationToken cancellationToken = default)
    {
        return await repository.AdicionarParticipanteAsync(
            dto.ProjetoID,
            dto.UsuarioID,
            dto.NmPapel,
            dto.UsuarioLogadoID,
            cancellationToken);
    }

    public async Task<int> AtualizarPapelParticipanteAsync(ParticipanteAtualizarPapelDto dto, CancellationToken cancellationToken = default)
    {
        return await repository.AtualizarPapelParticipanteAsync(
            dto.ProjetoParticipanteID,
            dto.NmPapel,
            dto.UsuarioLogadoID,
            cancellationToken);
    }

    public async Task<int> RemoverParticipanteAsync(int projetoParticipanteId, int usuarioLogadoId, CancellationToken cancellationToken = default)
    {
        return await repository.RemoverParticipanteAsync(projetoParticipanteId, usuarioLogadoId, cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────

    private static DateTime? ParseDate(string? value)
        => DateTime.TryParse(value, out var dt) ? dt : null;
}
