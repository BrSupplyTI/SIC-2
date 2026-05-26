using System.Net.Http.Json;
using SIC.Web.Models.Liberacao;

namespace SIC.Web.Services;

public sealed class LiberacaoPedidoApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<LiberacaoPedidoItemViewModel>> ListarAsync(
        int estabelecimentoId,
        int usuarioId,
        string? filtroPalavra1 = null,
        string? filtroPalavra2 = null,
        string? filtroPalavra3 = null,
        int filtroOrdemCompra = 0,
        int filtroRuptura = 0,
        int filtroFrete = 0,
        int filtroMargemNegativa = 0,
        decimal filtroValorAbaixo = 0,
        decimal filtroValorAcima = 0,
        string? filtroIntegracaoSAP = null,
        string? filtroContemItem = null,
        int filtroAtrasados = 0,
        int filtroFretePagar = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = new List<string>
            {
                $"estabelecimentoId={estabelecimentoId}",
                $"usuarioId={usuarioId}"
            };

            if (!string.IsNullOrWhiteSpace(filtroPalavra1)) qs.Add($"filtro.palavra1={Uri.EscapeDataString(filtroPalavra1)}");
            if (!string.IsNullOrWhiteSpace(filtroPalavra2)) qs.Add($"filtro.palavra2={Uri.EscapeDataString(filtroPalavra2)}");
            if (!string.IsNullOrWhiteSpace(filtroPalavra3)) qs.Add($"filtro.palavra3={Uri.EscapeDataString(filtroPalavra3)}");

            if (filtroOrdemCompra != 0) qs.Add($"filtro.filtroOrdemCompra={filtroOrdemCompra}");
            if (filtroRuptura != 0) qs.Add($"filtro.filtroRuptura={filtroRuptura}");
            if (filtroFrete != 0) qs.Add($"filtro.filtroFrete={filtroFrete}");
            if (filtroMargemNegativa != 0) qs.Add($"filtro.filtroMargemNegativa={filtroMargemNegativa}");
            if (filtroValorAbaixo != 0) qs.Add($"filtro.filtroValorAbaixo={filtroValorAbaixo}");
            if (filtroValorAcima != 0) qs.Add($"filtro.filtroValorAcima={filtroValorAcima}");
            if (!string.IsNullOrWhiteSpace(filtroIntegracaoSAP)) qs.Add($"filtro.filtroIntegracaoSAP={Uri.EscapeDataString(filtroIntegracaoSAP)}");
            if (!string.IsNullOrWhiteSpace(filtroContemItem)) qs.Add($"filtro.filtroContemItem={Uri.EscapeDataString(filtroContemItem)}");
            if (filtroAtrasados != 0) qs.Add($"filtro.filtroAtrasados={filtroAtrasados}");
            if (filtroFretePagar != 0) qs.Add($"filtro.filtroFretePagar={filtroFretePagar}");

            var url = $"api/liberacao-pedidos?{string.Join("&", qs)}";
            var data = await httpClient.GetFromJsonAsync<List<LiberacaoPedidoItemViewModel>>(url, cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool Sucesso, string Mensagem)> LiberarAsync(IEnumerable<int> cotacaoIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/liberacao-pedidos/liberar", new { CotacaoIds = cotacaoIds }, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AcaoResultado>(cancellationToken: cancellationToken);
                return (result?.Sucesso ?? false, result?.Mensagem ?? "Resposta inesperada.");
            }
            return (false, $"Erro HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao liberar pedidos: {ex.Message}");
        }
    }

    public async Task<(bool Sucesso, string Mensagem)> IntegrarAsync(IEnumerable<int> cotacaoIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/liberacao-pedidos/integrar", new { CotacaoIds = cotacaoIds }, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AcaoResultado>(cancellationToken: cancellationToken);
                return (result?.Sucesso ?? false, result?.Mensagem ?? "Resposta inesperada.");
            }
            return (false, $"Erro HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao integrar pedidos: {ex.Message}");
        }
    }

    public async Task<LiberacaoPedidoDetalhesViewModel?> GetDetalhesAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<LiberacaoPedidoDetalhesViewModel>(
                $"api/liberacao-pedidos/{cotacaoId}/detalhes",
                cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<LiberacaoPedidoAnaliseViewModel> AnalisarAsync(int cotacaoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<LiberacaoPedidoAnaliseViewModel>(
                $"api/liberacao-pedidos/{cotacaoId}/analise?usuarioId={usuarioId}",
                cancellationToken);
            return result ?? new LiberacaoPedidoAnaliseViewModel();
        }
        catch
        {
            return new LiberacaoPedidoAnaliseViewModel();
        }
    }

    // ======================================================================
    //  FASE 3 — Combos auxiliares (read-only)
    // ======================================================================

    public async Task<IReadOnlyList<LiberacaoPedidoComboItemViewModel>> ListarCanaisVendaAsync(int usuarioId, string nmCanalAtual, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoComboItemViewModel>(
            $"api/liberacao-pedidos/acoes/canais-venda?usuarioId={usuarioId}&nmCanalAtual={Uri.EscapeDataString(nmCanalAtual ?? string.Empty)}", ct);

    public async Task<IReadOnlyList<LiberacaoPedidoComboItemViewModel>> ListarCategoriasAsync(int clienteId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoComboItemViewModel>($"api/liberacao-pedidos/acoes/categorias?clienteId={clienteId}", ct);

    public async Task<IReadOnlyList<LiberacaoPedidoComboItemViewModel>> ListarCondicoesPagamentoAsync(string nmCondPagtoAtual, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoComboItemViewModel>(
            $"api/liberacao-pedidos/acoes/condicoes-pagamento?nmCondPagtoAtual={Uri.EscapeDataString(nmCondPagtoAtual ?? string.Empty)}", ct);

    public async Task<IReadOnlyList<LiberacaoPedidoFreteOpcaoViewModel>> ListarOpcoesFreteAsync(int cotacaoId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoFreteOpcaoViewModel>($"api/liberacao-pedidos/acoes/{cotacaoId}/opcoes-frete", ct);

    public async Task<IReadOnlyList<LiberacaoPedidoImpostoItemViewModel>> ListarImpostosAsync(int cotacaoId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoImpostoItemViewModel>($"api/liberacao-pedidos/acoes/{cotacaoId}/impostos", ct);

    // ---------- Logs (Fase 4) ----------

    public async Task<IReadOnlyList<LiberacaoPedidoCotLogViewModel>> ListarCotLogAsync(int cotacaoId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoCotLogViewModel>($"api/liberacao-pedidos/acoes/{cotacaoId}/cotlog", ct);

    public async Task<IReadOnlyList<LiberacaoPedidoBackOfficeLogViewModel>> ListarBackOfficeLogAsync(int cotacaoId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoBackOfficeLogViewModel>($"api/liberacao-pedidos/acoes/{cotacaoId}/backofficelog", ct);

    public async Task<IReadOnlyList<LiberacaoPedidoCotLogDetalhadoViewModel>> ListarCotLogDetalhadoAsync(int cotacaoId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoCotLogDetalhadoViewModel>($"api/liberacao-pedidos/acoes/{cotacaoId}/cotlog-detalhado", ct);

    // ---------- Itens (Fase 5) ----------

    public async Task<IReadOnlyList<LiberacaoPedidoItemBrSupplyViewModel>> ListarItensBrSupplyAsync(int cotacaoId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoItemBrSupplyViewModel>($"api/liberacao-pedidos/acoes/{cotacaoId}/itens-brsupply", ct);

    public async Task<IReadOnlyList<LiberacaoPedidoItemMarketplaceViewModel>> ListarItensMarketplaceAsync(int cotacaoId, CancellationToken ct = default)
        => await GetListAsync<LiberacaoPedidoItemMarketplaceViewModel>($"api/liberacao-pedidos/acoes/{cotacaoId}/itens-marketplace", ct);

    public async Task<LiberacaoPedidoTrocaCompativeisResultadoViewModel?> BuscarCompativeisTrocaAsync(int cotacaoItemId, CancellationToken ct = default)
        => await GetAsync<LiberacaoPedidoTrocaCompativeisResultadoViewModel>($"api/liberacao-pedidos/acoes/item/{cotacaoItemId}/compativeis", ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarItemAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/item/alterar", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarItemComOvAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/item/alterar-com-ov", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> ExcluirItemAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/item/excluir", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> TrocarItemAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/item/trocar", req, ct);

    // ======================================================================
    //  FASE 3 — Ações (POST JSON → retorna resultado padronizado)
    // ======================================================================

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarObsNotaAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/alterar-obs-nota", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarObsSolicitanteAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/alterar-obs-solicitante", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarObsAprovadorAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/alterar-obs-aprovador", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarOrdemCompraAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/alterar-ordem-compra", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarCanalVendaAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/alterar-canal-venda", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarCategoriaAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/alterar-categoria", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> AlterarCondPagtoAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/alterar-cond-pagto", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> CobrarFreteAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/cobrar-frete", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> LiberarMarketplaceModalAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/liberar-marketplace", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> CancelarPedidoAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/cancelar-pedido", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> CancelarMarketplaceAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/cancelar-marketplace", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> DesbloquearAlocacoesAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/desbloquear-alocacoes", req, ct);

    public Task<LiberacaoPedidoAcaoResultadoViewModel> GerarPedidoRupturasAsync(object req, CancellationToken ct = default)
        => PostAcaoAsync("api/liberacao-pedidos/acoes/gerar-pedido-rupturas", req, ct);

    // ---------- Helpers ----------

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<T>>(url, ct);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T>(url, ct);
        }
        catch
        {
            return default;
        }
    }

    private async Task<LiberacaoPedidoAcaoResultadoViewModel> PostAcaoAsync(string url, object req, CancellationToken ct)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(url, req, ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LiberacaoPedidoAcaoResultadoViewModel>(cancellationToken: ct);
                return result ?? new LiberacaoPedidoAcaoResultadoViewModel { Sucesso = false, Mensagem = "Resposta inesperada da API." };
            }
            return new LiberacaoPedidoAcaoResultadoViewModel { Sucesso = false, Mensagem = $"Erro HTTP {(int)response.StatusCode}." };
        }
        catch (Exception ex)
        {
            return new LiberacaoPedidoAcaoResultadoViewModel { Sucesso = false, Mensagem = $"Erro ao executar operação: {ex.Message}" };
        }
    }

    private sealed class AcaoResultado
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}
