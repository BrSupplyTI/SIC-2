namespace SIC.Web.Areas.Projetos.Models;

public sealed class ProjetoDetalhesViewModel
{
    public int ProjetoID { get; set; }
    public string NmProjeto { get; set; } = string.Empty;
    public string DsProjeto { get; set; } = string.Empty;
    public int ProjetoStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCorStatus { get; set; } = string.Empty;
    public string? DtInicio { get; set; }
    public string? DtPrevisaoFim { get; set; }
    public string? DtFimReal { get; set; }
    public int UsuarioCriadorID { get; set; }
    public string NmCriador { get; set; } = string.Empty;
    public string? DtCriacao { get; set; }
    public string? DtUltimaAtualizacao { get; set; }

    public int QtTarefas { get; set; }
    public int QtTarefasConcluidas { get; set; }

    public IReadOnlyList<ProjetoTarefaItemVm> Tarefas { get; set; } = [];
    public IReadOnlyList<ProjetoParticipanteItemVm> Participantes { get; set; } = [];
    public IReadOnlyList<ProjetoHistoricoItemVm> Historico { get; set; } = [];
    public IReadOnlyList<ProjetoStatusItemVm> StatusDisponiveis { get; set; } = [];
    public IReadOnlyList<ProjetoTarefaStatusItemVm> TarefaStatusDisponiveis { get; set; } = [];
    public IReadOnlyList<ProjetoTarefaPrioridadeItemVm> TarefaPrioridadesDisponiveis { get; set; } = [];

    public int UsuarioLogadoID { get; set; }
    public string NmUsuarioLogado { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
