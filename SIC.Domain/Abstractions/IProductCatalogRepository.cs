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
}
