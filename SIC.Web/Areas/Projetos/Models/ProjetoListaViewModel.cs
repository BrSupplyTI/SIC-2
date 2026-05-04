namespace SIC.Web.Areas.Projetos.Models;

public sealed class ProjetoListaViewModel
{
    public string? Texto { get; set; }
    public int ProjetoStatusID { get; set; }
    public string OrderBy { get; set; } = "Recentes";

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ProjetoItemVm> Itens { get; set; } = [];
    public IReadOnlyList<ProjetoStatusItemVm> StatusDisponiveis { get; set; } = [];

    public int UsuarioLogadoID { get; set; }
    public string NmUsuarioLogado { get; set; } = string.Empty;
    public string ModoVisualizacao { get; set; } = "quadro";

    // Dados extras para os modos Lista e Kanban
    public IReadOnlyList<ProjetoItemComTarefasVm> ItensComTarefas { get; set; } = [];
    public IReadOnlyList<ProjetoTarefaStatusItemVm> TarefaStatusDisponiveis { get; set; } = [];
    public IReadOnlyList<ProjetoTarefaPrioridadeItemVm> TarefaPrioridadesDisponiveis { get; set; } = [];
}
