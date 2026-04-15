namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoStatusDto
{
    public int ProjetoStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCor { get; set; } = string.Empty;
}
