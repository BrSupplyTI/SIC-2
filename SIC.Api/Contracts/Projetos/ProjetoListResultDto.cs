namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoListResultDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ProjetoListItemDto> Itens { get; set; } = [];
}
