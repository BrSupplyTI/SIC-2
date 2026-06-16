using System.Net.Http.Json;
using SIC.Web.Models.Abreviacoes;

namespace SIC.Web.Services.Abreviacoes;

public sealed class AbreviacaoApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<AbreviacaoItemViewModel>> BuscarDadosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<AbreviacaoItemViewModel>>("api/abreviacoes", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> GravarAsync(string texto, string abreviacao, int usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/abreviacoes",
                new { Texto = texto, Abreviacao = abreviacao, UsuarioId = usuarioId }, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/abreviacoes/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
