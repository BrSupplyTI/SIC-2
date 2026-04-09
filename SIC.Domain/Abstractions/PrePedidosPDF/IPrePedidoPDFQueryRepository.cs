using SIC.Domain.Entities.PrePedidosPDF;

namespace SIC.Domain.Abstractions.PrePedidosPDF;

/// <summary>
/// Operações de leitura do pré-pedido no banco de dados.
/// </summary>
public interface IPrePedidoPDFQueryRepository
{
    Task<IReadOnlyList<PrePedidoPDFListItem>> GetListAsync(
        int? status,
        string? cdExtCliente,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken = default);

    Task<PrePedidoPDFDetalhe?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrePedidoPDFLocalEntrega>> GetLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrePedidoPDFTrocaItem>> GetTrocaItensAsync(
        int tblPrecoId,
        int estabelecimentoId,
        int segmentoId,
        int familiaId,
        int itemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrePedidoPDFCatalogoItem>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default);

    Task<PrePedidoPDFInfoGerarPedido?> GetInfoGerarPedidoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrePedidoPDFInfoItemGerarPedido>> GetInfoItensGerarPedidoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);
}
