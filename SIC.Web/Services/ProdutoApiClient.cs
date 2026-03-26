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
}
