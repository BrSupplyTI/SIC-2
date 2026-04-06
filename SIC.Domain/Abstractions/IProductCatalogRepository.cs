namespace SIC.Domain.Abstractions;

using SIC.Domain.Entities;

public interface IProductCatalogRepository
{
    Task<IReadOnlyList<ProductCatalogItem>> GetCatalogAsync(
        int pageNumber,
        int pageSize,
        string? comecaComTexto,
        string? contemTexto,
        int flagAtivo,
        int flagMarcaPropria,
        int estabelecimentoId,
        int flagOutlet,
        int flagSobDemanda,
        int flagSustentavel,
        int flagNovidade,
        string? curva,
        int flagPadraoBrSupply,
        int flagComEstoque,
        string? orderBy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogEstablishment>> GetEstablishmentsAsync(CancellationToken cancellationToken = default);

    Task<ProductDetail?> GetProductDetailAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductStockEstablishment>> GetProductStockAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductStockAllocation>> GetProductStockAllocationsAsync(int itemId, int estabelecimentoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductPurchaseOrder>> GetProductPurchaseOrdersAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductSimilar>> GetProductSimilarsAsync(int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductSimilarStock>> GetProductSimilarStockAsync(int itemSimilarId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RelatedProduct>> GetRelatedProductsAsync(int itemId, CancellationToken cancellationToken = default);
}
