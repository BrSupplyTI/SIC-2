using System.Net.Http.Json;
using SIC.Web.Models.Clientes;
using SIC.Web.Models.Produtos;

namespace SIC.Web.Services;

public sealed class ClienteApiClient(HttpClient httpClient)
{
    public async Task<ClienteSearchResultVm?> SearchAsync(
        int pageNumber, int pageSize, string? comecaComTexto, string? contemTexto,
        int flagAtivo, int estabelecimentoId, int flagClienteMae,
        int carteiraId, int qtDiasUltimoPedido, string? orderBy,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"api/clientes/busca?PageNumber={pageNumber}&PageSize={pageSize}"
                   + $"&FlagAtivo={flagAtivo}&EstabelecimentoID={estabelecimentoId}"
                   + $"&FlagClienteMae={flagClienteMae}&CarteiraID={carteiraId}"
                   + $"&QtDiasUltimoPedido={qtDiasUltimoPedido}"
                   + $"&UsuarioID={usuarioId}";

            if (!string.IsNullOrWhiteSpace(comecaComTexto))
                qs += $"&ComecaComTexto={Uri.EscapeDataString(comecaComTexto)}";
            if (!string.IsNullOrWhiteSpace(contemTexto))
                qs += $"&ContemTexto={Uri.EscapeDataString(contemTexto)}";
            if (!string.IsNullOrWhiteSpace(orderBy))
                qs += $"&OrderBy={Uri.EscapeDataString(orderBy)}";

            return await httpClient.GetFromJsonAsync<ClienteSearchResultVm>(qs, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ClienteDetalhesViewModel?> GetClientDetailAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ClienteDetalhesViewModel>($"api/clientes/{clienteId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<CarteiraVm>> GetWalletsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<CarteiraVm>>("api/clientes/carteiras", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<CatalogEstablishmentVm>> GetEstablishmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<CatalogEstablishmentVm>>("api/clientes/estabelecimentos", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ConsultorVm>> GetConsultantsAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ConsultorVm>>($"api/clientes/{clienteId}/consultores", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ClientTitleVm>> GetTitulosAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ClientTitleVm>>($"api/clientes/{clienteId}/titulos", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }
}
