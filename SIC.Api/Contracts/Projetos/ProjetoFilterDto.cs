namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Texto { get; set; }
    public int ProjetoStatusID { get; set; }
    public string? OrderBy { get; set; } = "Recentes";
    public bool ExcluirEncerrados { get; set; } = true;
}
