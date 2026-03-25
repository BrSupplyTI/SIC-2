using SIC.Api.Contracts.Pedidos;

namespace SIC.Api.Services;

public interface IOrderSearchService
{
    Task<OrderSearchResultDto> SearchByOrderNumberAsync(string? numeroPedido, CancellationToken cancellationToken = default);
    Task<OrderHeaderDetailsDto?> GetOrderHeaderDetailsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderSapIntegrationItemDto>> GetOrderSapIntegrationAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderTaxItemDto>> GetOrderTaxesAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FreightCalculationItemDto>> GetFreightCalculationHistoryAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FreightCalculationItemDto>> GetFreightCalculationAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderBrSupplyItemDto>> GetOrderBrSupplyItemsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderBrSupplyItemDto>> GetOrderBrSupplyItemsRupturaAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderBrSupplyItemDto>> GetOrderMarketplaceItemsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderApprovalItemDto>> GetOrderApprovalItemsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderInvoiceItemDto>> GetOrderInvoiceItemsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderRomaneioItemDto>> GetOrderRomaneiosAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderTrackingItemDto>> GetOrderTrackingAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderVolumeColetaItemDto>> GetVolumesColetaAsync(string pedCli, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderTicketItemDto>> GetOrderTicketsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<OrderCreditAnalysisDto?> GetOrderCreditAnalysisAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderValidationItemDto>> GetOrderValidationsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderLogItemDto>> GetOrderLogsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<OrderSearchResultDto> SearchByPurchaseOrderAsync(string? ordemCompra, CancellationToken cancellationToken = default);
    Task<OrderSearchResultDto> SearchByInvoiceAsync(string? notaFiscal, int? serie, CancellationToken cancellationToken = default);
    Task<string?> GetInvoiceXmlAsync(string chaveDanfe, CancellationToken cancellationToken = default);
}
