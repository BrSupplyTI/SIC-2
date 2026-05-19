using SIC.Domain.Entities.Cotacao;

namespace SIC.Domain.Abstractions.Cotacao;

/// <summary>
/// Operações de escrita da Cotação no banco de dados.
/// </summary>
public interface ICotacaoCommandRepository
{
    Task<(bool Success, string? Error)> AdicionarItemAsync(
        int propostaId,
        string codItemBR,
        string descrItemBR,
        string tipoCusto,
        decimal precoItem,
        decimal vlrCustoAquisicao,
        decimal vlrCustoMedio,
        int quantidade,
        decimal vlrPrecoMinimo,
        decimal vlrTabelaPreco,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> CalcularMargemItemAsync(
        int propostaId,
        int propostaItemId,
        string type,
        string viaTela,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> AtualizarItemAsync(
        int propostaId,
        int propostaItemId,
        decimal precoUnitario,
        decimal quantidade,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> AtualizarCustoItemAsync(
        int propostaId,
        int propostaItemId,
        string tipoCusto,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> GerarItensAsync(
        int propostaId,
        string tipoGeracao,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> RemoverItensAsync(
        int propostaId,
        IReadOnlyList<(int PropostaItemId, string CdItem)> itens,
        string motivo,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> SalvarCondPagtoAsync(
        int propostaId,
        int condPagtoId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> RecalcularMargemBrutaPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finaliza a proposta (StatusID = 2) ou envia para aprovação (StatusID = 10)
    /// caso o atendente precise de aprovação e a margem bruta esteja fora do intervalo.
    /// Retorna o StatusID resultante.
    /// </summary>
    Task<(bool Success, int? StatusId, string? Error)> FinalizarAsync(
        int propostaId,
        string dataValidade,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> AprovarAsync(
        int propostaId,
        int aprovadorId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> ReprovarAsync(
        int propostaId,
        int aprovadorId,
        string justificativa,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> SalvarFretePropostaAsync(
        int propostaId,
        int transportadoraId,
        decimal valorFrete,
        int prazoTotal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Encerra envios pendentes e executa BR_sp_Proposta_GeraPedido_LocalEntrega.
    /// Retorna o CotacaoID gerado, ou null se não houver.
    /// </summary>
    Task<(bool Success, int? CotacaoId, string? Error)> AutorizarFaturamentoAsync(
        int propostaId,
        string ipAprovacao,
        CancellationToken cancellationToken = default);

    Task<int> CriarPropostaAsync(
        CriarPropostaRequest request,
        CancellationToken cancellationToken = default);

    Task AtualizarPropostaAsync(
        int propostaId,
        CriarPropostaRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoLocalEntregaOption>> EnsureLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task SalvarLogEnvioAsync(
        SalvarLogEnvioRequest request,
        CancellationToken cancellationToken = default);
}
