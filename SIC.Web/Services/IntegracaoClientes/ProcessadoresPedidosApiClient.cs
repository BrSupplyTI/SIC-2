using System.Net.Http.Json;
using SIC.Web.Models.IntegracaoClientes.ProcessadoresPedidos;

namespace SIC.Web.Services.IntegracaoClientes;

public sealed class ProcessadoresPedidosApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<ProcessadorPedidoOptionVm>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProcessadorPedidoOptionVm>>("api/integracao-clientes/processadores-pedidos", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ProcessadorPedidoConfiguracaoVm>> GetConfiguracoesAsync(int processadorPedidoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProcessadorPedidoConfiguracaoVm>>($"api/integracao-clientes/processadores-pedidos/{processadorPedidoId}/configuracoes", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }
}
