using SIC.Api.Contracts.Produtos;

namespace SIC.Api.Services;

public interface IProductCatalogService
{
    Task<ProductCatalogResultDto> GetCatalogAsync(ProductCatalogFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogEstablishmentDto>> GetEstablishmentsAsync(CancellationToken cancellationToken = default);
    Task<ProductDetailDto?> GetDetailAsync(int itemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductStockEstablishmentDto>> GetStockAsync(int itemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductStockAllocationDto>> GetStockAllocationsAsync(int itemId, int estabelecimentoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductPurchaseOrderDto>> GetPurchaseOrdersAsync(int itemId, CancellationToken cancellationToken = default);
}
