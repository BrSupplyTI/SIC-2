using System.Net.Http.Json;
using SIC.Web.Models.Pedidos;

namespace SIC.Web.Services;

public sealed class PedidoApiClient(HttpClient httpClient)
{
    public Task<OrderSearchResultVm?> SearchOrderByNumberAsync(string? numeroPedido, CancellationToken cancellationToken = default)
        => SendOrderSearchAsync("api/pedidos/buscar-por-pedido", new { NumeroPedido = numeroPedido }, cancellationToken);

    public Task<OrderSearchResultVm?> SearchOrderByPurchaseOrderAsync(string? ordemCompra, CancellationToken cancellationToken = default)
        => SendOrderSearchAsync("api/pedidos/buscar-por-ordem-compra", new { OrdemCompra = ordemCompra }, cancellationToken);

    public Task<OrderSearchResultVm?> SearchOrderByInvoiceAsync(string? notaFiscal, int? serie, CancellationToken cancellationToken = default)
        => SendOrderSearchAsync("api/pedidos/buscar-por-nota-fiscal", new { NotaFiscal = notaFiscal, Serie = serie }, cancellationToken);

    public async Task<OrderHeaderDetailsVm?> GetOrderHeaderDetailsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<OrderHeaderDetailsVm>($"api/pedidos/{pedido}/detalhes-cabecalho", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<OrderSapIntegrationItemVm>> GetOrderSapIntegrationAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderSapIntegrationItemVm>>($"api/pedidos/{pedido}/integracao-sap", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderTaxItemVm>> GetOrderTaxesAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderTaxItemVm>>($"api/pedidos/{pedido}/impostos", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<FreightCalculationItemVm>> GetFreightCalculationHistoryAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<FreightCalculationItemVm>>($"api/pedidos/{pedido}/historico-frete", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<FreightCalculationItemVm>> GetFreightCalculationAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<FreightCalculationItemVm>>($"api/pedidos/{pedido}/calculo-frete", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderBrSupplyItemVm>> GetOrderBrSupplyItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderBrSupplyItemVm>>($"api/pedidos/{pedido}/itens-br-supply", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderBrSupplyItemVm>> GetOrderMarketplaceItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderBrSupplyItemVm>>($"api/pedidos/{pedido}/itens-marketplace", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderBrSupplyItemVm>> GetOrderBrSupplyItemsRupturaAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderBrSupplyItemVm>>($"api/pedidos/{pedido}/itens-br-supply-ruptura", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderApprovalItemVm>> GetOrderApprovalItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderApprovalItemVm>>($"api/pedidos/{pedido}/logs-aprovacao", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderInvoiceItemVm>> GetOrderInvoiceItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderInvoiceItemVm>>($"api/pedidos/{pedido}/notas-fiscais", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderRomaneioItemVm>> GetOrderRomaneiosAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderRomaneioItemVm>>($"api/pedidos/{pedido}/romaneios", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderTrackingItemVm>> GetOrderTrackingAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderTrackingItemVm>>($"api/pedidos/{pedido}/tracking", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderVolumeColetaItemVm>> GetVolumesColetaAsync(string pedCli, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderVolumeColetaItemVm>>($"api/pedidos/volumes-coleta?pedCli={Uri.EscapeDataString(pedCli)}", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderTicketItemVm>> GetOrderTicketsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderTicketItemVm>>($"api/pedidos/{pedido}/chamados", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<OrderCreditAnalysisVm?> GetOrderCreditAnalysisAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<OrderCreditAnalysisVm>($"api/pedidos/{pedido}/analise-credito", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<OrderValidationItemVm>> GetOrderValidationsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderValidationItemVm>>($"api/pedidos/{pedido}/validacoes", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<OrderLogItemVm>> GetOrderLogsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<OrderLogItemVm>>($"api/pedidos/{pedido}/registros-logs", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<byte[]?> GetInvoiceXmlAsync(string chave, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"api/pedidos/nf-xml/{chave}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<OrderSearchResultVm?> SendOrderSearchAsync<TRequest>(string path, TRequest payload, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(path, payload, cancellationToken);
            if (response.Content.Headers.ContentLength == 0)
            {
                return new OrderSearchResultVm
                {
                    Success = false,
                    ErrorCode = "EMPTY_RESPONSE",
                    Message = $"Falha na operação. Status HTTP {(int)response.StatusCode}."
                };
            }

            return await response.Content.ReadFromJsonAsync<OrderSearchResultVm>(cancellationToken: cancellationToken);
        }
        catch
        {
            return new OrderSearchResultVm
            {
                Success = false,
                ErrorCode = "API_UNAVAILABLE",
                Message = "Não foi possível conectar na API do SIC."
            };
        }
    }
}
