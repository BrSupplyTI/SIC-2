namespace SIC.Domain.Entities;

public sealed class ProjetoTarefaStatus
{
    public int ProjetoTarefaStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCor { get; set; } = string.Empty;
    public int NrOrdem { get; set; }
}
