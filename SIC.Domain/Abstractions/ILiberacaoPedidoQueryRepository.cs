using SIC.Domain.Entities.Liberacao;

namespace SIC.Domain.Abstractions;

/// <summary>
/// Queries auxiliares para a tela de Liberação de Pedido (combos, fretes, impostos).
/// </summary>
public interface ILiberacaoPedidoQueryRepository
{
    Task<IReadOnlyList<LiberacaoPedidoComboItem>> ListarCanaisVendaAsync(int usuarioId, string nmCanalVendaAtual, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoComboItem>> ListarCategoriasAsync(int clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoComboItem>> ListarCondicoesPagamentoAsync(string nmCondPagtoAtual, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoFreteOpcao>> ListarOpcoesFreteAsync(int cotacaoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoImpostoItem>> ListarImpostosAsync(int cotacaoId, CancellationToken cancellationToken = default);

    // ---------- Logs (Fase 4) ----------
    Task<IReadOnlyList<LiberacaoPedidoCotLog>> ListarCotLogAsync(int cotacaoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoBackOfficeLog>> ListarBackOfficeLogAsync(int cotacaoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoCotLogDetalhado>> ListarCotLogDetalhadoAsync(int cotacaoId, CancellationToken cancellationToken = default);

    // ---------- Itens (Fase 5) ----------
    Task<IReadOnlyList<LiberacaoPedidoItemBrSupply>> ListarItensBrSupplyAsync(int cotacaoId, CancellationToken ct = default);
    Task<IReadOnlyList<LiberacaoPedidoItemMarketplace>> ListarItensMarketplaceAsync(int cotacaoId, CancellationToken ct = default);
    Task<LiberacaoPedidoTrocaCompativeisResultado> BuscarCompativeisTrocaAsync(int cotacaoItemId, CancellationToken ct = default);
}
