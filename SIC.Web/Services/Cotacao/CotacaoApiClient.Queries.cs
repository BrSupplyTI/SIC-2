using System.Net.Http.Json;
using SIC.Web.Models.Cotacao;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Operações de leitura (queries) da Cotação via API.
/// </summary>
public sealed partial class CotacaoApiClient
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        try { return await httpClient.GetFromJsonAsync<T>(url, ct); }
        catch { return default; }
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<T>>(url, ct);
            return data ?? [];
        }
        catch { return []; }
    }

    // ── catálogo ─────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<CotacaoCatalogoItemViewModel>> BuscarCatalogoAsync(
        int propostaId,
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        var qs = $"?descricao={Uri.EscapeDataString(descricao)}&clienteId={clienteId}&tblPrecoId={tblPrecoId}&estabelecimentoId={estabelecimentoId}";
        return GetListAsync<CotacaoCatalogoItemViewModel>(
            $"api/cotacao/{propostaId}/buscar-catalogo{qs}", cancellationToken);
    }

    // ── lista ─────────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<CotacaoListItemViewModel>> GetListaAsync(
        int? usuarioId,
        int filtroCotacao,
        string? cdExtCliente,
        int? propostaId,
        string? cnpj,
        int? estabelecimentoId,
        int? statusId,
        DateTime dataInicial,
        DateTime dataFinal,
        CancellationToken cancellationToken = default)
    {
        var qs = $"?filtroCotacao={filtroCotacao}"
            + (usuarioId.HasValue ? $"&usuarioId={usuarioId}" : "")
            + (cdExtCliente is not null ? $"&cdExtCliente={Uri.EscapeDataString(cdExtCliente)}" : "")
            + (propostaId.HasValue ? $"&propostaId={propostaId}" : "")
            + (cnpj is not null ? $"&cnpj={Uri.EscapeDataString(cnpj)}" : "")
            + (estabelecimentoId.HasValue ? $"&estabelecimentoId={estabelecimentoId}" : "")
            + (statusId.HasValue ? $"&statusId={statusId}" : "")
            + $"&dataInicial={dataInicial:yyyy-MM-dd}&dataFinal={dataFinal:yyyy-MM-dd}";

        return GetListAsync<CotacaoListItemViewModel>($"api/cotacao/lista{qs}", cancellationToken);
    }

    // ── detalhe / itens ───────────────────────────────────────────────────────

    public Task<CotacaoDetalheViewModel?> GetDetalheAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetAsync<CotacaoDetalheViewModel>($"api/cotacao/{propostaId}", cancellationToken);

    public Task<IReadOnlyList<CotacaoItemViewModel>> GetItensAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoItemViewModel>($"api/cotacao/{propostaId}/itens", cancellationToken);

    // ── impostos / validação ──────────────────────────────────────────────────

    public Task<CotacaoItemImpostosViewModel?> GetImpostosItemAsync(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken = default)
        => GetAsync<CotacaoItemImpostosViewModel>(
            $"api/cotacao/{propostaId}/itens/{propostaItemId}/impostos", cancellationToken);

    public Task<IReadOnlyList<CotacaoItemValidacaoViewModel>> ValidarItensImportacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoItemValidacaoViewModel>(
            $"api/cotacao/{propostaId}/validar-itens-importacao", cancellationToken);

    // ── options gerais ────────────────────────────────────────────────────────

    public Task<IReadOnlyList<CotacaoEstabelecimentoOptionViewModel>> GetEstabelecimentoOptionsAsync(
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoEstabelecimentoOptionViewModel>(
            "api/cotacao/options/estabelecimentos", cancellationToken);

    public Task<IReadOnlyList<CotacaoSelectOptionViewModel>> GetStatusOptionsAsync(
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoSelectOptionViewModel>(
            "api/cotacao/options/status", cancellationToken);

    public Task<IReadOnlyList<CotacaoSelectOptionViewModel>> GetCondicoesPagamentoAsync(
        int estabelecimentoId,
        decimal valorTotal,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoSelectOptionViewModel>(
            $"api/cotacao/options/condicoes-pagamento?estabelecimentoId={estabelecimentoId}&valorTotal={valorTotal}",
            cancellationToken);

    public Task<IReadOnlyList<CotacaoSelectOptionViewModel>> GetFormasPagamentoAsync(
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoSelectOptionViewModel>(
            "api/cotacao/options/formas-pagamento", cancellationToken);

    public Task<IReadOnlyList<CotacaoTipoOptionViewModel>> GetTiposCotacaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoTipoOptionViewModel>(
            $"api/cotacao/options/tipos-cotacao?usuarioId={usuarioId}", cancellationToken);

    public Task<IReadOnlyList<CotacaoSelectOptionViewModel>> GetMotivosBonificacaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoSelectOptionViewModel>(
            $"api/cotacao/options/motivos-bonificacao?usuarioId={usuarioId}", cancellationToken);

    public Task<IReadOnlyList<CotacaoEstabelecimentoOptionViewModel>> GetEstabelecimentosAsync(
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoEstabelecimentoOptionViewModel>(
            "api/cotacao/options/estabelecimentos-add", cancellationToken);

    public Task<IReadOnlyList<CotacaoUfOptionViewModel>> GetUfsAsync(
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoUfOptionViewModel>(
            "api/cotacao/options/ufs", cancellationToken);

    public Task<IReadOnlyList<CotacaoSelectOptionViewModel>> GetCidadesByUfAsync(
        string cdUf,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoSelectOptionViewModel>(
            $"api/cotacao/options/cidades?cdUf={Uri.EscapeDataString(cdUf)}", cancellationToken);

    public Task<IReadOnlyList<CotacaoSelectOptionViewModel>> GetTiposOrdemAsync(
        int cotacaoTipoId,
        int usuarioId,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoSelectOptionViewModel>(
            $"api/cotacao/options/tipos-ordem?cotacaoTipoId={cotacaoTipoId}&usuarioId={usuarioId}",
            cancellationToken);

    // ── clientes ──────────────────────────────────────────────────────────────

    public async Task<int?> GetFormaPagamentoByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<FormaPagamentoClienteVm>(
            $"api/cotacao/clientes/{clienteId}/forma-pagamento", cancellationToken);
        return result?.FormaPagamentoSAP;
    }

    private sealed class FormaPagamentoClienteVm
    {
        public int? FormaPagamentoSAP { get; set; }
    }

    public async Task<string?> GetTipoOVSAPByEnderecoAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<TipoOVSAPVm>(
            $"api/cotacao/enderecos/{clienteEnderecoId}/tipo-ovsap", cancellationToken);
        return result?.TipoOVSAP;
    }

    private sealed class TipoOVSAPVm
    {
        public string? TipoOVSAP { get; set; }
    }

    public Task<IReadOnlyList<ClienteLookupItemViewModel>> SearchClientesAsync(
        string termo,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
        => GetListAsync<ClienteLookupItemViewModel>(
            $"api/cotacao/clientes/buscar?termo={Uri.EscapeDataString(termo)}&estabelecimentoId={estabelecimentoId}",
            cancellationToken);

    public Task<IReadOnlyList<ClienteEnderecoLookupViewModel>> GetEnderecosByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
        => GetListAsync<ClienteEnderecoLookupViewModel>(
            $"api/cotacao/clientes/{clienteId}/enderecos", cancellationToken);

    public Task<CotacaoTabelaPrecoOptionViewModel?> GetTabelaPrecoByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
        => GetAsync<CotacaoTabelaPrecoOptionViewModel>(
            $"api/cotacao/clientes/{clienteId}/tabela-preco", cancellationToken);

    public Task<IReadOnlyList<CotacaoContratoOptionViewModel>> GetContratosByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoContratoOptionViewModel>(
            $"api/cotacao/clientes/{clienteId}/contratos", cancellationToken);

    // ── endereços ─────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<ClienteLocalEntregaLookupViewModel>> GetLocaisEntregaByEnderecoAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default)
        => GetListAsync<ClienteLocalEntregaLookupViewModel>(
            $"api/cotacao/enderecos/{clienteEnderecoId}/locais-entrega", cancellationToken);

    public Task<CotacaoFreteInicialViewModel?> BuscarFreteInicialAsync(
        int clienteEnderecoId,
        int clienteId,
        string? ufDestino,
        CancellationToken cancellationToken = default)
    {
        var qs = $"?clienteId={clienteId}"
            + (ufDestino is not null ? $"&ufDestino={Uri.EscapeDataString(ufDestino)}" : "");
        return GetAsync<CotacaoFreteInicialViewModel>(
            $"api/cotacao/enderecos/{clienteEnderecoId}/frete-inicial{qs}", cancellationToken);
    }

    // ── frete ─────────────────────────────────────────────────────────────────

    public Task<IReadOnlyList<FreteOpcaoViewModel>> CalcularFretePropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetListAsync<FreteOpcaoViewModel>(
            $"api/cotacao/{propostaId}/calcular-frete", cancellationToken);

    // ── e-mail ────────────────────────────────────────────────────────────────

    public Task<EnviarEmailCotacaoViewModel?> GetEmailDadosAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetAsync<EnviarEmailCotacaoViewModel>(
            $"api/cotacao/{propostaId}/email-dados", cancellationToken);

    public Task<IReadOnlyList<CotacaoEnvioHistoricoItemViewModel>> GetHistoricoEnviosAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetListAsync<CotacaoEnvioHistoricoItemViewModel>(
            $"api/cotacao/{propostaId}/historico-envios", cancellationToken);

    public Task<CotacaoEmailTemplateViewModel?> GetEmailTemplateAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetAsync<CotacaoEmailTemplateViewModel>(
            $"api/cotacao/{propostaId}/email-template", cancellationToken);

    // ── edição ────────────────────────────────────────────────────────────────

    public Task<CotacaoEditDadosViewModel?> GetPropostaParaEditAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
        => GetAsync<CotacaoEditDadosViewModel>(
            $"api/cotacao/{propostaId}/edit-dados", cancellationToken);

    // ── executivo de vendas ───────────────────────────────────────────────────

    public async Task<string> GetExecutivoVendasAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<ExecutivoVendasVm>(
            $"api/cotacao/clientes/{clienteId}/executivo-vendas", cancellationToken);
        return result?.Executivo ?? string.Empty;
    }

    private sealed class ExecutivoVendasVm
    {
        public string Executivo { get; set; } = string.Empty;
    }

    // ── configuração SMTP ──────────────────────────────────────────────────────

    public async Task<SmtpConfigDto?> GetSmtpConfigAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<SmtpConfigDto>(
            "api/configuration/smtp", cancellationToken);
    }

    public sealed class SmtpConfigDto
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
