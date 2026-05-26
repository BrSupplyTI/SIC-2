using SIC.Api.Contracts.Liberacao;

namespace SIC.Api.Services;

public interface ILiberacaoPedidoAcoesService
{
    // Queries auxiliares (combos + fretes + impostos)
    Task<IReadOnlyList<LiberacaoPedidoComboItemDto>> ListarCanaisVendaAsync(int usuarioId, string nmCanalAtual, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoComboItemDto>> ListarCategoriasAsync(int clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoComboItemDto>> ListarCondicoesPagamentoAsync(string nmCondPagtoAtual, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoFreteOpcaoDto>> ListarOpcoesFreteAsync(int cotacaoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LiberacaoPedidoImpostoItemDto>> ListarImpostosAsync(int cotacaoId, CancellationToken cancellationToken = default);

    // Logs (Fase 4)
    Task<IReadOnlyList<LiberacaoPedidoCotLogDto>> ListarCotLogAsync(int cotacaoId, CancellationToken ct = default);
    Task<IReadOnlyList<LiberacaoPedidoBackOfficeLogDto>> ListarBackOfficeLogAsync(int cotacaoId, CancellationToken ct = default);
    Task<IReadOnlyList<LiberacaoPedidoCotLogDetalhadoDto>> ListarCotLogDetalhadoAsync(int cotacaoId, CancellationToken ct = default);

    // Itens (Fase 5) - queries
    Task<IReadOnlyList<LiberacaoPedidoItemBrSupplyDto>> ListarItensBrSupplyAsync(int cotacaoId, CancellationToken ct = default);
    Task<IReadOnlyList<LiberacaoPedidoItemMarketplaceDto>> ListarItensMarketplaceAsync(int cotacaoId, CancellationToken ct = default);
    Task<LiberacaoPedidoTrocaCompativeisResultadoDto> BuscarCompativeisTrocaAsync(int cotacaoItemId, CancellationToken ct = default);

    // Itens (Fase 5) - commands
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarItemAsync(AlterarItemRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarItemComOvAsync(AlterarItemComOvRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> ExcluirItemAsync(ExcluirItemRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> TrocarItemAsync(TrocarItemRequest req, CancellationToken ct = default);

    // Ações
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarObsNotaAsync(AlterarObservacaoRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarObsSolicitanteAsync(AlterarObservacaoRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarObsAprovadorAsync(AlterarObservacaoRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarOrdemCompraAsync(AlterarOrdemCompraRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarCanalVendaAsync(AlterarCanalVendaRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarCategoriaAsync(AlterarCategoriaRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> AlterarCondPagtoAsync(AlterarCondPagtoRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> CobrarFreteAsync(CobrarFreteRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> LiberarMarketplaceAsync(LiberarMarketplaceRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> CancelarPedidoAsync(CancelarPedidoRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> CancelarMarketplaceAsync(CancelarPedidoRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> DesbloquearAlocacoesAsync(DesbloquearAlocacoesRequest req, CancellationToken ct = default);
    Task<LiberacaoPedidoAcaoResultadoDto> GerarPedidoRupturasAsync(GerarPedidoRupturasRequest req, CancellationToken ct = default);
}
