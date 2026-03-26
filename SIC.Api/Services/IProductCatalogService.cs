using SIC.Api.Contracts.Produtos;

namespace SIC.Api.Services;

public interface IProductCatalogService
{
    Task<ProductCatalogResultDto> GetCatalogAsync(ProductCatalogFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogEstablishmentDto>> GetEstablishmentsAsync(CancellationToken cancellationToken = default);
}
