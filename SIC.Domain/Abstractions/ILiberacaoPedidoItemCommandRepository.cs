using SIC.Domain.Entities.Liberacao;

namespace SIC.Domain.Abstractions;

/// <summary>
/// Comandos de escrita de itens do pedido (operações do arquivo comercial_liberacao_updates.php).
/// Cada método executa validações server-side antes do UPDATE/DELETE, em transação.
/// </summary>
public interface ILiberacaoPedidoItemCommandRepository
{
    /// <summary>
    /// ALTERAR_ITEM — altera qt/valor/ordem/sequência e re-aloca estoque.
    /// Valida FlagAloca (não pode ser 2=atendido) e se pedido já tem OV/fila SAP.
    /// Retorna Mensagem de erro amigável ou null em caso de sucesso.
    /// </summary>
    Task<string?> AlterarItemAsync(
        int cotacaoId, int cotacaoItemId, int itemIdOld, string cdItemOld, string nmItemOld,
        int qtNova, int qtAntiga, decimal vlrNovo, decimal vlrAntigo,
        string ordemNova, string ordemAntiga, string sequenciaNova, string sequenciaAntiga,
        string motivo, int usuarioId, CancellationToken ct = default);

    /// <summary>
    /// ALTERAR_ITEM_COM_OV — pedido com OV, só permite alterar OrdemCliente/SequenciaCliente.
    /// </summary>
    Task<string?> AlterarItemComOvAsync(
        int cotacaoId, int cotacaoItemId, string cdItemOld, string nmItemOld,
        string ordemNova, string ordemAntiga, string sequenciaNova, string sequenciaAntiga,
        string motivo, int usuarioId, CancellationToken ct = default);

    /// <summary>
    /// EXCLUIR_ITEM — remove o item do pedido e re-aloca estoque.
    /// Valida FlagAloca e OV. Retorna mensagem de erro ou null em sucesso.
    /// </summary>
    Task<string?> ExcluirItemAsync(
        int cotacaoId, int cotacaoItemId, int itemIdOld, string cdItemOld, string nmItemOld,
        decimal qtAntiga, decimal vlrAntigo,
        string motivo, int usuarioId, int estabelecimentoId,
        CancellationToken ct = default);

    /// <summary>
    /// TROCAR_ITEM — substitui um item do pedido por outro; opcionalmente grava em
    /// BR_ItensTrocaAutomatica_PorCliente quando flagTrocaAutomatica=1.
    /// </summary>
    Task<string?> TrocarItemAsync(
        int cotacaoId, int cotacaoItemId, int itemIdOld, string cdItemOld, string nmItemOld,
        int itemSubstitutoId, bool flagTrocaAutomatica,
        string motivo, int usuarioId, int estabelecimentoId,
        CancellationToken ct = default);
}
