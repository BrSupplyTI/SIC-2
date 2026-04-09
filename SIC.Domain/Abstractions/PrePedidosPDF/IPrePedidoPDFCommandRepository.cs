namespace SIC.Domain.Abstractions.PrePedidosPDF;

/// <summary>
/// Operações de escrita do pré-pedido no banco de dados.
/// </summary>
public interface IPrePedidoPDFCommandRepository
{
    Task<bool> AtualizarEnderecoAsync(
        int prePedidoId,
        int clienteEnderecoId,
        string logradouro,
        CancellationToken cancellationToken = default);

    Task<bool> AtualizarLocalEntregaAsync(
        int prePedidoId,
        int clienteLocalEntregaId,
        string nomeLocalEntrega,
        CancellationToken cancellationToken = default);

    Task<bool> AtualizarCnpjAsync(
        int prePedidoId,
        string cnpj,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateQuantidadeAsync(
        int prePedidoItemId,
        int prePedidoId,
        int quantidade,
        string descricao,
        CancellationToken cancellationToken = default);

    Task<bool> CancelarAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirItemAsync(
        int prePedidoItemId,
        int prePedidoId,
        string descricao,
        CancellationToken cancellationToken = default);

    Task<bool> GravarTrocaItemAsync(
        int prePedidoItemId,
        int prePedidoId,
        string cdItem,
        int itemId,
        string nomeItem,
        decimal vlrTabelaPreco,
        string cdItemAntigo,
        string descricaoAntiga,
        string valorAntigo,
        string motivoTrocaItem,
        CancellationToken cancellationToken = default);

    Task<bool> AdicionarItemAsync(
        int prePedidoId,
        string cdItem,
        string descricao,
        int quantidade,
        decimal vlrTabelaPreco,
        string cdItemCliente,
        int itemId,
        string ordemCompra,
        CancellationToken cancellationToken = default);

    Task<bool> SetProcessadorPraZeroAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);

    Task<bool> InserirLogReprocessamentoAsync(
        int prePedidoId,
        string mensagem,
        CancellationToken cancellationToken = default);

    Task<bool> AtualizarStatusAguardandoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);

    Task<int> GerarPedidoAsync(
        int estabelecimentoId,
        int clienteId,
        int clienteEnderecoId,
        string cnpj,
        int clienteLocalEntregaId,
        int clienteUsuarioId,
        int naturezaOperacaoId,
        int condPagtoId,
        string ordemCompra,
        int? clienteCategoriaPedidoId,
        CancellationToken cancellationToken = default);

    Task<bool> AtualizarCotacaoStatusAsync(
        int prePedidoId,
        int cotacaoId,
        CancellationToken cancellationToken = default);

    Task<bool> GerarItemPedidoAsync(
        int cotacaoId,
        int tipo,
        int itemId,
        int qtItem,
        decimal vlrUnit,
        string cdItemCliente,
        string ordemCliente,
        int seqCliente,
        CancellationToken cancellationToken = default);
}
