using SIC.Api.Contracts.Liberacao;

namespace SIC.Api.Services;

public interface ILiberacaoPedidoService
{
    Task<IReadOnlyList<LiberacaoPedidoItemDto>> ListarAsync(
        int estabelecimentoId,
        int usuarioId,
        LiberacaoPedidoFilterDto filtro,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o cabeçalho completo de um pedido para a tela de detalhes (read-only).
    /// Retorna null se o pedido não for encontrado.
    /// </summary>
    Task<LiberacaoPedidoDetalheDto?> ObterDetalhesAsync(
        int cotacaoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa a análise de liberação (SIC_AnaliseLiberacaoPedido) e retorna erros/alertas
    /// já expandidos em listas individuais, além de indicar se o pedido está pronto.
    /// </summary>
    Task<LiberacaoPedidoAnaliseDto> AnalisarAsync(
        int cotacaoId,
        int usuarioId,
        CancellationToken cancellationToken = default);
}
