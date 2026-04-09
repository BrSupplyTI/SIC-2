namespace SIC.Domain.Abstractions.PrePedidosPDF;

/// <summary>
/// Integrações externas usadas pelo fluxo do pré-pedido.
/// </summary>
public interface IPrePedidoPDFIntegrationService
{
    Task<string> GetConteudoArquivoPedidoAsync(
        string cdExtCliente,
        string ordemCompra,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> ReprocessarPedidoAsync(
        string jsonPedido,
        CancellationToken cancellationToken = default);
}