using SIC.Api.Models.Auth;

namespace SIC.Api.Services.PrePedidosPDF;

/// <summary>
/// Operações de escrita do pré-pedido:
/// cancelar, reprocessar, gerar pedido, validar aceite,
/// atualizar endereço/local de entrega/CNPJ,
/// adicionar/excluir/atualizar itens, trocar item.
/// </summary>
public interface IPrePedidoPDFCommandService
{
    Task<OperationResult> AtualizarEnderecoAsync(
        int prePedidoId,
        int clienteEnderecoId,
        string logradouro,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarLocalEntregaAsync(
        int prePedidoId,
        int clienteLocalEntregaId,
        string nomeLocalEntrega,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarCnpjAsync(
        int prePedidoId,
        string cnpj,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarQuantidadeAsync(
        int prePedidoId,
        int prePedidoItemId,
        int quantidade,
        string descricao,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarVlrUnitAsync(
        int prePedidoId,
        int prePedidoItemId,
        decimal vlrUnit,
        string descricao,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarObsAsync(
        int prePedidoId,
        string obsNota,
        string obsComprador,
        CancellationToken cancellationToken = default);

    Task<OperationResult> CancelarAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> ExcluirItemAsync(
        int prePedidoId,
        int prePedidoItemId,
        string descricao,
        CancellationToken cancellationToken = default);

    Task<OperationResult> TrocarItemAsync(
        int prePedidoId,
        int prePedidoItemId,
        string cdItem,
        int itemId,
        string nomeItem,
        decimal vlrTabelaPreco,
        string cdItemAntigo,
        string descricaoAntiga,
        string valorAntigo,
        string motivoTrocaItem,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AdicionarItemAsync(
        int prePedidoId,
        string codItemBR,
        string descrItemBR,
        int quantidade,
        decimal precoTbl,
        string itemDePara,
        int itemId,
        string ordemCompra,
        CancellationToken cancellationToken = default);

    Task<OperationResult> ReprocessarAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AceitarPedidoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default);
}
