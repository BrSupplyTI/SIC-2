using System.Net.Http.Json;
using SIC.Web.Models.Auth;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Operações de escrita (commands) da Cotação via API.
/// </summary>
public sealed partial class CotacaoApiClient
{
    public Task<OperationResultVm> AdicionarItemAsync(
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
        => PostAsync(
            $"api/cotacao/{propostaId}/itens/adicionar",
            new
            {
                CodItemBR = codItemBR,
                DescrItemBR = descrItemBR,
                TipoCusto = tipoCusto,
                PrecoItem = precoItem,
                VlrCustoAquisicao = vlrCustoAquisicao,
                VlrCustoMedio = vlrCustoMedio,
                Quantidade = quantidade,
                VlrPrecoMinimo = vlrPrecoMinimo,
                VlrTabelaPreco = vlrTabelaPreco,
            },
            cancellationToken);

    public Task<OperationResultVm> AtualizarItemAsync(
        int propostaId,
        int propostaItemId,
        decimal precoUnitario,
        decimal quantidade,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/itens/{propostaItemId}/atualizar",
            new { PrecoUnitario = precoUnitario, Quantidade = quantidade },
            cancellationToken);

    public Task<OperationResultVm> AtualizarCustoItemAsync(
        int propostaId,
        int propostaItemId,
        string tipoCusto,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/itens/{propostaItemId}/atualizar-custo",
            new { TipoCusto = tipoCusto },
            cancellationToken);

    public Task<OperationResultVm> CalcularMargemItemAsync(
        int propostaId,
        int propostaItemId,
        string type,
        string viaTela,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/itens/{propostaItemId}/calcular-margem",
            new { Type = type, ViaTela = viaTela },
            cancellationToken);

    public Task<OperationResultVm> GerarItensAsync(
        int propostaId,
        string tipoGeracao,
        int usuarioId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/gerar-itens",
            new { TipoGeracao = tipoGeracao, UsuarioID = usuarioId },
            cancellationToken);

    public Task<OperationResultVm> RemoverItensAsync(
        int propostaId,
        IEnumerable<(int PropostaItemId, string CdItem)> itens,
        string motivo,
        int usuarioId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/itens/remover",
            new
            {
                Itens     = itens.Select(i => new { i.PropostaItemId, i.CdItem }),
                Motivo    = motivo,
                UsuarioId = usuarioId
            },
            cancellationToken);

    public Task<OperationResultVm> SalvarCondPagtoAsync(
        int propostaId,
        int condPagtoId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/salvar-cond-pagto",
            new { CondPagtoId = condPagtoId },
            cancellationToken);

    public Task<OperationResultVm> RecalcularMargemBrutaPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/recalcular-margem-bruta",
            new { },
            cancellationToken);

    private async Task<OperationResultVm> PostAsync<TRequest>(
        string url,
        TRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(url, request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<OperationResultVm>(cancellationToken: cancellationToken);

            return result ?? new OperationResultVm
            {
                Success = false,
                Message = "Resposta inválida da API."
            };
        }
        catch
        {
            return new OperationResultVm
            {
                Success = false,
                Message = "Não foi possível conectar na API do SIC."
            };
        }
    }
}
