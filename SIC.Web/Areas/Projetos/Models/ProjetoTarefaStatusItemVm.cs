namespace SIC.Web.Areas.Projetos.Models;

public sealed class ProjetoTarefaStatusItemVm
{
    public int ProjetoTarefaStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCor { get; set; } = string.Empty;
    public int NrOrdem { get; set; }
}
