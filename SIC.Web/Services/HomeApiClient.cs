using System.Net.Http.Json;
using SIC.Web.Models.Home;

namespace SIC.Web.Services;

public sealed class HomeApiClient(HttpClient httpClient)
{
    public async Task<List<ShortcutVm>> GetUserShortcutsAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<ShortcutVm>>($"api/home/atalhos/usuario/{usuarioId}", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<ShortcutVm>> GetAllShortcutsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<ShortcutVm>>("api/home/atalhos", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> AddUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/home/atalhos/usuario", new { UsuarioID = usuarioId, AtalhoID = atalhoId }, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/home/atalhos/usuario/{usuarioId}/{atalhoId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<CurrencyQuoteVm>> GetCurrencyQuotesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<CurrencyQuoteVm>>("api/home/cotacoes", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<WeatherInfoVm?> GetWeatherInfoAsync(int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<WeatherInfoVm>($"api/home/clima/{estabelecimentoId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
