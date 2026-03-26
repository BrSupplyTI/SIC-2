namespace SIC.Api.Contracts.Produtos;

public sealed class ProductCatalogResultDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ProductCatalogItemDto> Itens { get; set; } = [];
}
