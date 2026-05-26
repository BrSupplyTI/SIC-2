using SIC.Domain.Entities.Liberacao;

namespace SIC.Domain.Abstractions;

public interface ILiberacaoPedidoDetalheRepository
{
    /// <summary>
    /// Executa SIC_DetalhesLiberacaoPedido e retorna o cabeçalho do pedido.
    /// Retorna null se o pedido não existir.
    /// </summary>
    Task<LiberacaoPedidoDetalhe?> ObterAsync(int cotacaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa SIC_Parametros_ClienteEndereco e retorna parâmetros do cliente.
    /// </summary>
    Task<LiberacaoPedidoParametrosCliente?> ObterParametrosClienteAsync(int cotacaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa SIC_AnaliseLiberacaoPedido e retorna as linhas de erro/alerta.
    /// </summary>
    Task<IReadOnlyList<LiberacaoPedidoAnalise>> AnalisarAsync(int cotacaoId, int usuarioId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Parâmetros do cliente/endereço (SP SIC_Parametros_ClienteEndereco).
/// </summary>
public sealed class LiberacaoPedidoParametrosCliente
{
    public decimal Taxa { get; set; }
    public decimal Minimo { get; set; }
    public decimal Bloqueio { get; set; }
    public int FlagNaoEditarPedidoComOC { get; set; }
}
