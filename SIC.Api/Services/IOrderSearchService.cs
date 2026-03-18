using SIC.Api.Contracts.Pedidos;

namespace SIC.Api.Services;

public interface IOrderSearchService
{
    Task<OrderSearchResultDto> SearchByOrderNumberAsync(string? numeroPedido, CancellationToken cancellationToken = default);
    Task<OrderHeaderDetailsDto?> GetOrderHeaderDetailsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<OrderSearchResultDto> SearchByPurchaseOrderAsync(string? ordemCompra, CancellationToken cancellationToken = default);
    Task<OrderSearchResultDto> SearchByInvoiceAsync(string? notaFiscal, int? serie, CancellationToken cancellationToken = default);
}
