namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoTarefaPrioridadeDto
{
    public int ProjetoTarefaPrioridadeID { get; set; }
    public string NmPrioridade { get; set; } = string.Empty;
    public string CdCor { get; set; } = string.Empty;
}
