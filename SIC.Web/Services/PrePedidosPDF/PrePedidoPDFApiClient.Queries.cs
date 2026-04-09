using System.Net.Http.Json;
using SIC.Web.Models.PrePedidosPDF;

namespace SIC.Web.Services.PrePedidosPDF;

/// <summary>
/// Operações de leitura (queries) do pré-pedido.
/// Métodos: GetListAsync, GetByIdAsync, GetLocaisEntregaAsync, GetTrocaItensAsync, BuscarCatalogoAsync,
///          GetItensAsync, GetLogsAsync.
/// </summary>
public sealed partial class PrePedidoPDFApiClient
{
    public async Task<IReadOnlyList<PrePedidoPDFListItemViewModel>> GetListAsync(
        int? status,
        string? cdExtCliente,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new List<string>();

            if (status.HasValue)
                query.Add($"status={status.Value}");

            if (!string.IsNullOrWhiteSpace(cdExtCliente))
                query.Add($"cdExtCliente={Uri.EscapeDataString(cdExtCliente)}");

            if (dataInicial.HasValue)
                query.Add($"dataInicial={dataInicial.Value:yyyy-MM-dd}");

            if (dataFinal.HasValue)
                query.Add($"dataFinal={dataFinal.Value:yyyy-MM-dd}");

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

            var data = await httpClient.GetFromJsonAsync<List<PrePedidoPDFListItemViewModel>>(
                $"api/pre-pedidos-pdf{qs}", cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<PrePedidoPDFDetalhesViewModel?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<PrePedidoPDFDetalhesViewModel>(
                $"api/pre-pedidos-pdf/{id}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<PrePedidoPDFLocalEntregaViewModel>> GetLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<PrePedidoPDFLocalEntregaViewModel>>(
                $"api/pre-pedidos-pdf/locais-entrega?clienteEnderecoId={clienteEnderecoId}",
                cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<PrePedidoPDFTrocaItemViewModel>> GetTrocaItensAsync(
        int tblPrecoId,
        int estabelecimentoId,
        int segmentoId,
        int familiaId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<PrePedidoPDFTrocaItemViewModel>>(
                $"api/pre-pedidos-pdf/troca-itens?tblPrecoId={tblPrecoId}&estabelecimentoId={estabelecimentoId}&segmentoId={segmentoId}&familiaId={familiaId}&itemId={itemId}",
                cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<PrePedidoPDFCatalogoItemViewModel>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"?descricao={Uri.EscapeDataString(descricao)}&clienteId={clienteId}&tblPrecoId={tblPrecoId}&estabelecimentoId={estabelecimentoId}";
            var data = await httpClient.GetFromJsonAsync<List<PrePedidoPDFCatalogoItemViewModel>>(
                $"api/pre-pedidos-pdf/buscar-catalogo{qs}", cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<PrePedidoPDFItemViewModel>> GetItensAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<PrePedidoPDFItemViewModel>>(
                $"api/pre-pedidos-pdf/{id}/itens", cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<PrePedidoPDFLogViewModel>> GetLogsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<PrePedidoPDFLogViewModel>>(
                $"api/pre-pedidos-pdf/{id}/logs", cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }
}
