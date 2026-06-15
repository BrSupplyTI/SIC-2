using SIC.Api.Contracts.Cotacao;
using SIC.Api.Models.Auth;

namespace SIC.Api.Services.Cotacao;

/// <summary>
/// Operações de escrita da Cotação.
/// </summary>
public interface ICotacaoCommandService
{
    Task<OperationResult> AdicionarItemAsync(
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

    Task<OperationResult> CalcularMargemItemAsync(
        int propostaId,
        int propostaItemId,
        string type,
        string viaTela,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarItemAsync(
        int propostaId,
        int propostaItemId,
        decimal precoUnitario,
        decimal quantidade,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarCustoItemAsync(
        int propostaId,
        int propostaItemId,
        string tipoCusto,
        CancellationToken cancellationToken = default);

    Task<OperationResult> GerarItensAsync(
        int propostaId,
        string tipoGeracao,
        int usuarioId,
        string? cotacaoId = null,
        CancellationToken cancellationToken = default);

    Task<OperationResult> RemoverItensAsync(
        int propostaId,
        IReadOnlyList<(int PropostaItemId, string CdItem)> itens,
        string motivo,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> SalvarCondPagtoAsync(
        int propostaId,
        int condPagtoId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> RecalcularMargemBrutaPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> FinalizarAsync(
        int propostaId,
        string dataValidade,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AprovarAsync(
        int propostaId,
        int aprovadorId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> ReprovarAsync(
        int propostaId,
        int aprovadorId,
        string justificativa,
        CancellationToken cancellationToken = default);

    Task<OperationResult> SalvarFretePropostaAsync(
        int propostaId,
        int transportadoraId,
        decimal valorFrete,
        int prazoTotal,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AutorizarFaturamentoAsync(
        int propostaId,
        string ipAprovacao,
        CancellationToken cancellationToken = default);

    Task<int> CriarPropostaAsync(
        CriarPropostaRequest request,
        CancellationToken cancellationToken = default);

    Task AtualizarPropostaAsync(
        int propostaId,
        AtualizarPropostaRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoLocalEntregaOptionDto>> EnsureLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task SalvarLogEnvioAsync(
        SalvarLogEnvioRequest request,
        CancellationToken cancellationToken = default);
}
