namespace SIC.Web.Areas.Projetos.Models;

public sealed class ProjetoListaComTarefasViewModel
{
    public string? Texto { get; set; }
    public int ProjetoStatusID { get; set; }
    public string OrderBy { get; set; } = "Recentes";

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ProjetoItemComTarefasVm> Itens { get; set; } = [];
}
