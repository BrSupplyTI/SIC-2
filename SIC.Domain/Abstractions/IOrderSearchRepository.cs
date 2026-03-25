namespace SIC.Domain.Abstractions;

using SIC.Domain.Entities;

public interface IOrderSearchRepository
{
    Task<bool> ExistsOrderByNumberAsync(int numeroPedido, CancellationToken cancellationToken = default);
    Task<OrderHeaderDetails?> GetOrderHeaderDetailsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderSapIntegrationItem>> GetOrderSapIntegrationAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderTaxItem>> GetOrderTaxesAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FreightCalculationItem>> GetFreightCalculationHistoryAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FreightCalculationItem>> GetFreightCalculationAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderBrSupplyItem>> GetOrderBrSupplyItemsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderBrSupplyItem>> GetOrderBrSupplyItemsRupturaAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderBrSupplyItem>> GetOrderMarketplaceItemsAsync(int pedido, CancellationToken cancellationToken = default);    
    Task<IReadOnlyList<OrderApprovalItem>> GetOrderApprovalItemsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderInvoiceItem>> GetOrderInvoiceItemsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderRomaneioItem>> GetOrderRomaneiosAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderTrackingItem>> GetOrderTrackingAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderVolumeColetaItem>> GetVolumesColetaAsync(string pedCli, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderTicketItem>> GetOrderTicketsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<OrderCreditAnalysis?> GetOrderCreditAnalysisAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderValidationItem>> GetOrderValidationsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderLogItem>> GetOrderLogsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<PurchaseOrderSearchResult> SearchByPurchaseOrderAsync(string ordemCompra, CancellationToken cancellationToken = default);
    Task<int?> GetOrderIdByInvoiceAsync(string notaFiscal, int serie, CancellationToken cancellationToken = default);
    Task<string?> GetInvoiceXmlAsync(string chaveDanfe, CancellationToken cancellationToken = default);
}
