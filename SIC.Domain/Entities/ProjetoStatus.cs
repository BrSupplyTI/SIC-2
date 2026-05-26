namespace SIC.Domain.Entities;

public sealed class ProjetoStatus
{
    public int ProjetoStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCor { get; set; } = string.Empty;
    public int NrOrdem { get; set; }
}
