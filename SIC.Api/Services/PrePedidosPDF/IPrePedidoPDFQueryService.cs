using SIC.Api.Contracts.PrePedidosPDF;

namespace SIC.Api.Services.PrePedidosPDF;

/// <summary>
/// Operações de leitura do pré-pedido.
/// </summary>
public interface IPrePedidoPDFQueryService
{
    Task<IReadOnlyList<PrePedidoPDFListItemDto>> GetListAsync(
        int? status,
        string? cdExtCliente,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken = default);

    Task<PrePedidoPDFDetalhesDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrePedidoPDFLocalEntregaDto>> GetLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrePedidoPDFTrocaItemDto>> GetTrocaItensAsync(
        int tblPrecoId,
        int estabelecimentoId,
        int segmentoId,
        int familiaId,
        int itemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PrePedidoPDFCatalogoItemDto>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default);
}
