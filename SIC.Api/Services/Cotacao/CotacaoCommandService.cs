using SIC.Api.Models.Auth;
using SIC.Domain.Abstractions.Cotacao;

namespace SIC.Api.Services.Cotacao;

/// <summary>
/// Implementação das operações de escrita da Cotação.
/// </summary>
public sealed class CotacaoCommandService(ICotacaoCommandRepository repository) : ICotacaoCommandService
{
    public async Task<OperationResult> AdicionarItemAsync(
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
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.AdicionarItemAsync(
            propostaId,
            codItemBR,
            descrItemBR,
            tipoCusto,
            precoItem,
            vlrCustoAquisicao,
            vlrCustoMedio,
            quantidade,
            vlrPrecoMinimo,
            vlrTabelaPreco,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Item adicionado com sucesso." : (error ?? "Erro ao adicionar item à cotação.")
        };
    }

    public async Task<OperationResult> CalcularMargemItemAsync(
        int propostaId,
        int propostaItemId,
        string type,
        string viaTela,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.CalcularMargemItemAsync(
            propostaId,
            propostaItemId,
            type,
            viaTela,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Margem calculada com sucesso." : (error ?? "Erro ao calcular margem.")
        };
    }

    public async Task<OperationResult> AtualizarItemAsync(
        int propostaId,
        int propostaItemId,
        decimal precoUnitario,
        decimal quantidade,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.AtualizarItemAsync(
            propostaId,
            propostaItemId,
            precoUnitario,
            quantidade,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Item atualizado com sucesso." : (error ?? "Erro ao atualizar item.")
        };
    }
    public async Task<OperationResult> AtualizarCustoItemAsync(
        int propostaId,
        int propostaItemId,
        string tipoCusto,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.AtualizarCustoItemAsync(
            propostaId,
            propostaItemId,
            tipoCusto,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Custo atualizado com sucesso." : (error ?? "Erro ao atualizar custo do item.")
        };
    }
    public async Task<OperationResult> GerarItensAsync(
        int propostaId,
        string tipoGeracao,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.GerarItensAsync(
            propostaId,
            tipoGeracao,
            usuarioId,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Itens gerados com sucesso." : (error ?? "Erro ao gerar itens.")
        };
    }

    public async Task<OperationResult> RemoverItensAsync(
        int propostaId,
        IReadOnlyList<(int PropostaItemId, string CdItem)> itens,
        string motivo,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.RemoverItensAsync(
            propostaId, itens, motivo, usuarioId, cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Item(ns) removido(s) com sucesso." : (error ?? "Erro ao remover item(ns).")
        };
    }

    public async Task<OperationResult> SalvarCondPagtoAsync(
        int propostaId,
        int condPagtoId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.SalvarCondPagtoAsync(
            propostaId, condPagtoId, cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Condição de pagamento salva com sucesso." : (error ?? "Erro ao salvar condição de pagamento.")
        };
    }

    public async Task<OperationResult> RecalcularMargemBrutaPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.RecalcularMargemBrutaPropostaAsync(
            propostaId, cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Margem bruta recalculada com sucesso." : (error ?? "Erro ao recalcular margem bruta.")
        };
    }
}
