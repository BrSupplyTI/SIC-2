using System.Net.Http.Json;
using SIC.Web.Models.InteligenciaDeBusca;

namespace SIC.Web.Services.InteligenciaDeBusca;

public sealed class CategorizacaoApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<CategorizacaoItemViewModel>> GetItensCategorizadosAsync(
        int? estabelecimentoId, CancellationToken ct = default)
    {
        try
        {
            var url = "api/categorizacao/itens";
            if (estabelecimentoId is > 0) url += $"?estabelecimentoId={estabelecimentoId}";
            var data = await httpClient.GetFromJsonAsync<List<CategorizacaoItemViewModel>>(url, ct);
            return data ?? [];
        }
        catch { return []; }
    }

    public async Task<IReadOnlyList<CategorizacaoItemSemCategoriaViewModel>> GetItensSemCategoriaAsync(
        CancellationToken ct = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<CategorizacaoItemSemCategoriaViewModel>>(
                "api/categorizacao/itens-sem-categoria", ct);
            return data ?? [];
        }
        catch { return []; }
    }

    public async Task<IReadOnlyList<CategoriaViewModel>> GetCategoriasAsync(CancellationToken ct = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<CategoriaViewModel>>(
                "api/categorizacao/categorias", ct);
            return data ?? [];
        }
        catch { return []; }
    }

    public async Task<bool> SalvarCategoriaAsync(int itemId, int pesquisaTipoListaId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/categorizacao/salvar-categoria",
                new { itemID = itemId, pesquisaTipoListaID = pesquisaTipoListaId }, ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
    public async Task<bool> RemoverCategoriaAsync(int itemId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/categorizacao/remover-categoria/{itemId}", ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
