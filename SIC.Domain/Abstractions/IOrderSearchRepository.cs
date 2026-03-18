namespace SIC.Domain.Abstractions;

using SIC.Domain.Entities;

public interface IOrderSearchRepository
{
    Task<bool> ExistsOrderByNumberAsync(int numeroPedido, CancellationToken cancellationToken = default);
    Task<OrderHeaderDetails?> GetOrderHeaderDetailsAsync(int pedido, CancellationToken cancellationToken = default);
    Task<PurchaseOrderSearchResult> SearchByPurchaseOrderAsync(string ordemCompra, CancellationToken cancellationToken = default);
    Task<int?> GetOrderIdByInvoiceAsync(string notaFiscal, int serie, CancellationToken cancellationToken = default);
}
