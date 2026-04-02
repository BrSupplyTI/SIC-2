using System.Net.Http.Json;
using SIC.Web.Models.Produtos;

namespace SIC.Web.Services;

public sealed class ProdutoApiClient(HttpClient httpClient)
{
    public async Task<ProductCatalogResultVm?> GetCatalogAsync(
        int pageNumber, int pageSize, string? comecaComTexto, string? contemTexto,
        int flagAtivo, int flagMarcaPropria, int estabelecimentoId,
        int flagOutlet, int flagSobDemanda, int flagSustentavel,
        int flagNovidade, string? curva, int flagPadraoBrSupply, int flagComEstoque,
        string? orderBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"api/produtos/catalogo?PageNumber={pageNumber}&PageSize={pageSize}"
                   + $"&FlagAtivo={flagAtivo}&EstabelecimentoID={estabelecimentoId}"
                   + $"&FlagMarcaPropria={flagMarcaPropria}"
                   + $"&FlagOutlet={flagOutlet}&FlagSobDemanda={flagSobDemanda}"
                   + $"&FlagSustentavel={flagSustentavel}&FlagNovidade={flagNovidade}"
                   + $"&FlagPadraoBrSupply={flagPadraoBrSupply}&FlagComEstoque={flagComEstoque}";

            if (!string.IsNullOrWhiteSpace(comecaComTexto))
                qs += $"&ComecaComTexto={Uri.EscapeDataString(comecaComTexto)}";
            if (!string.IsNullOrWhiteSpace(contemTexto))
                qs += $"&ContemTexto={Uri.EscapeDataString(contemTexto)}";
            if (!string.IsNullOrWhiteSpace(curva))
                qs += $"&Curva={Uri.EscapeDataString(curva)}";
            if (!string.IsNullOrWhiteSpace(orderBy))
                qs += $"&OrderBy={Uri.EscapeDataString(orderBy)}";

            return await httpClient.GetFromJsonAsync<ProductCatalogResultVm>(qs, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<CatalogEstablishmentVm>> GetEstablishmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<CatalogEstablishmentVm>>("api/produtos/estabelecimentos", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ProdutoDetalhesViewModel?> GetProductDetailAsync(int itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ProdutoDetalhesViewModel>($"api/produtos/{itemId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<ProdutoEstoqueEstabelecimentoVm>> GetProductStockAsync(int itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProdutoEstoqueEstabelecimentoVm>>($"api/produtos/{itemId}/estoques", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ProdutoAlocacaoEstoqueVm>> GetProductStockAllocationsAsync(int itemId, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProdutoAlocacaoEstoqueVm>>($"api/produtos/{itemId}/estoques/{estabelecimentoId}/alocacoes", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ProdutoOrdemCompraVm>> GetProductPurchaseOrdersAsync(int itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProdutoOrdemCompraVm>>($"api/produtos/{itemId}/ordens-compra", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }
}
