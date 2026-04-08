namespace SIC.Api.Contracts.Clientes;

public sealed class ClientSearchResultDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ClientSearchItemDto> Itens { get; set; } = [];
}
