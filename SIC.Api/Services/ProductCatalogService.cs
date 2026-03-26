using SIC.Api.Contracts.Produtos;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class ProductCatalogService(IProductCatalogRepository repository) : IProductCatalogService
{
    public async Task<ProductCatalogResultDto> GetCatalogAsync(ProductCatalogFilterDto filter, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCatalogAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.ComecaComTexto,
            filter.ContemTexto,
            filter.FlagAtivo,
            filter.FlagMarcaPropria,
            filter.EstabelecimentoID,
            filter.FlagOutlet,
            filter.FlagSobDemanda,
            filter.FlagSustentavel,
            filter.FlagNovidade,
            filter.Curva,
            filter.FlagPadraoBrSupply,
            filter.FlagComEstoque,
            filter.OrderBy,
            cancellationToken);

        var totalRegistros = items.Count > 0 ? items[0].TotalRegistros : 0;

        var dtos = items.Select(i => new ProductCatalogItemDto
        {
            ItemID = i.ItemID,
            CdItem = i.CdItem,
            NmItem = i.NmItem,
            NmSegmento = i.NmSegmento,
            NmFamilia = i.NmFamilia,
            NmSubFamilia = i.NmSubFamilia,
            NmMarca = i.NmMarca,
            FlagTipoMarca = i.FlagTipoMarca,
            NumCA = i.NumCA,
            QtEstoque = i.QtEstoque,
            Curva = i.Curva,
            DtCadastro = i.DtCadastro?.ToString("dd/MM/yyyy")
        }).ToList();

        return new ProductCatalogResultDto
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filter.PageSize) : 0,
            Itens = dtos
        };
    }

    public async Task<IReadOnlyList<CatalogEstablishmentDto>> GetEstablishmentsAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetEstablishmentsAsync(cancellationToken);
        return items.Select(e => new CatalogEstablishmentDto
        {
            EstabelecimentoID = e.EstabelecimentoID,
            NmEstabelecimento = e.NmEstabelecimento
        }).ToList();
    }
}
