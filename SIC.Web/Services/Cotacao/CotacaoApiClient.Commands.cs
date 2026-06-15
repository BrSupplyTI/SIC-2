using System.Net.Http.Json;
using SIC.Web.Models.Auth;
using SIC.Web.Models.Cotacao;

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
        string? cotacaoId = null,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/gerar-itens",
            new { TipoGeracao = tipoGeracao, UsuarioID = usuarioId, CotacaoID = cotacaoId },
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

    // ── proposta ──────────────────────────────────────────────────────────────

    public async Task<int?> CriarPropostaAsync(
        CriarPropostaRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/cotacao", request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<CriarPropostaResultVm>(
                cancellationToken: cancellationToken);
            return result?.PropostaId;
        }
        catch { return null; }
    }

    public async Task<bool> AtualizarPropostaAsync(
        int propostaId,
        AtualizarPropostaRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/cotacao/{propostaId}", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── finalizar / aprovar / reprovar ────────────────────────────────────────

    public Task<OperationResultVm> FinalizarAsync(
        int propostaId,
        string dataValidade,
        int usuarioId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/finalizar",
            new { DataValidade = dataValidade, UsuarioId = usuarioId },
            cancellationToken);

    public Task<OperationResultVm> AprovarAsync(
        int propostaId,
        int aprovadorId,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/aprovar",
            new { AprovadorId = aprovadorId },
            cancellationToken);

    public Task<OperationResultVm> ReprovarAsync(
        int propostaId,
        int aprovadorId,
        string justificativa,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/reprovar",
            new { AprovadorId = aprovadorId, Justificativa = justificativa },
            cancellationToken);

    // ── frete / faturamento ───────────────────────────────────────────────────

    public Task<OperationResultVm> SalvarFreteAsync(
        int propostaId,
        int transportadoraId,
        decimal valorFrete,
        int prazoTotal,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/salvar-frete",
            new { TransportadoraId = transportadoraId, ValorFrete = valorFrete, PrazoTotal = prazoTotal },
            cancellationToken);

    public Task<OperationResultVm> AutorizarFaturamentoAsync(
        int propostaId,
        string ipAprovacao,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/cotacao/{propostaId}/autorizar-faturamento",
            new { IpAprovacao = ipAprovacao },
            cancellationToken);

    // ── locais de entrega ─────────────────────────────────────────────────────

    public async Task<bool> EnsureLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"api/cotacao/enderecos/{clienteEnderecoId}/ensure-locais-entrega",
                new { },
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── log de envio de e-mail ────────────────────────────────────────────────

    public async Task<bool> SalvarLogEnvioAsync(
        SalvarLogEnvioRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"api/cotacao/{request.PropostaId}/salvar-log-envio",
                request,
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private sealed class CriarPropostaResultVm
    {
        public int PropostaId { get; set; }
    }

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
