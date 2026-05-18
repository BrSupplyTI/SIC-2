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
}
