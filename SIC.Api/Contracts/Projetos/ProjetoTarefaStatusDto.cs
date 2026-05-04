namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoTarefaStatusDto
{
    public int ProjetoTarefaStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCor { get; set; } = string.Empty;
    public int NrOrdem { get; set; }
}
