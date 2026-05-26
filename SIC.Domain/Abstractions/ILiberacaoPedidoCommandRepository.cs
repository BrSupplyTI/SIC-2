namespace SIC.Domain.Abstractions;

/// <summary>
/// Comandos de escrita da tela de Liberação de Pedido. Cada método executa UPDATE + logs em transação.
/// Retorna mensagem de sucesso ou exceção em caso de falha.
/// </summary>
public interface ILiberacaoPedidoCommandRepository
{
    Task AlterarObsNotaAsync(int cotacaoId, int usuarioId, string obsAntiga, string obsNova, string motivo, CancellationToken cancellationToken = default);
    Task AlterarObsSolicitanteAsync(int cotacaoId, int usuarioId, string obsAntiga, string obsNova, string motivo, CancellationToken cancellationToken = default);
    Task AlterarObsAprovadorAsync(int cotacaoId, int usuarioId, string obsAntiga, string obsNova, string motivo, CancellationToken cancellationToken = default);
    Task AlterarOrdemCompraAsync(int cotacaoId, int usuarioId, string ordemAntiga, string ordemNova, string motivo, CancellationToken cancellationToken = default);
    Task AlterarCanalVendaAsync(int cotacaoId, int usuarioId, string nmCanalAntigo, int canalVendaIdNovo, string motivo, CancellationToken cancellationToken = default);
    Task AlterarCategoriaAsync(int cotacaoId, int usuarioId, string nmCategoriaAntiga, int categoriaIdNova, string motivo, CancellationToken cancellationToken = default);
    Task AlterarCondPagtoAsync(int cotacaoId, int usuarioId, string nmCondPagtoAntiga, int condPagtoIdNova, string motivo, CancellationToken cancellationToken = default);
    Task CobrarFreteAsync(int cotacaoId, int usuarioId, decimal vlrFrete, int flagFreteServico, CancellationToken cancellationToken = default);
    Task LiberarMarketplaceAsync(int cotacaoId, int usuarioId, CancellationToken cancellationToken = default);
    Task CancelarPedidoAsync(int cotacaoId, int usuarioId, string motivo, CancellationToken cancellationToken = default);
    Task CancelarMarketplaceAsync(int cotacaoId, int usuarioId, string motivo, CancellationToken cancellationToken = default);
    Task DesbloquearAlocacoesAsync(int cotacaoId, int usuarioId, string motivo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gera um novo pedido com os itens em ruptura (BR_sp_GerarPedidoComRupturas).
    /// Retorna o novo CotacaoID ou null se a SP não gerou pedido filho.
    /// </summary>
    Task<int?> GerarPedidoRupturasAsync(int cotacaoId, int clienteId, int clienteUsuarioId, int usuarioId, string motivo, CancellationToken cancellationToken = default);
}
