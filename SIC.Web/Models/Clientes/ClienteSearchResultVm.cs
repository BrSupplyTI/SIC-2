namespace SIC.Web.Models.Clientes;

public sealed class ClienteSearchResultVm
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ClienteSearchItemVm> Itens { get; set; } = [];
}
