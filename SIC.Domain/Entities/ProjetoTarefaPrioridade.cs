namespace SIC.Domain.Entities;

public sealed class ProjetoTarefaPrioridade
{
    public int ProjetoTarefaPrioridadeID { get; set; }
    public string NmPrioridade { get; set; } = string.Empty;
    public string CdCor { get; set; } = string.Empty;
    public int NrOrdem { get; set; }
}
