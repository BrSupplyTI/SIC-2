using SIC.Api.Contracts.Cotacao;

namespace SIC.Api.Services.Cotacao;

/// <summary>
/// Operações de leitura da Cotação.
/// </summary>
public interface ICotacaoQueryService
{
    Task<IReadOnlyList<CotacaoCatalogoItemDto>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoListItemDto>> GetListAsync(
        int? usuarioId,
        int filtroCotacao,
        string? cdExtCliente,
        int? propostaId,
        string? cnpj,
        int? estabelecimentoId,
        int? statusId,
        DateTime dataInicial,
        DateTime dataFinal,
        CancellationToken cancellationToken = default);

    Task<CotacaoDetalheDto?> GetByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoDetalheItemDto>> GetItensByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOptionDto>> GetEstabelecimentoOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOptionDto>> GetStatusOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOptionDto>> GetCondicoesPagamentoAsync(
        int estabelecimentoId,
        decimal valorTotal,
        CancellationToken cancellationToken = default);

    Task<string> GetExecutivoVendasAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoFreteOpcaoDto>> CalcularFretePropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<CotacaoItemImpostosDto?> GetImpostosItemAsync(
        int propostaItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoItemValidacaoDto>> ValidarItensImportacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<CotacaoDadosEmailDto?> GetEnviarEmailDadosAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoEnvioHistoricoItemDto>> GetHistoricoEnviosAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoTipoOptionDto>> GetTiposCotacaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOptionDto>> GetMotivosBonificacaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoEstabelecimentoOptionDto>> GetEstabelecimentosAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoUfOptionDto>> GetUfsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoClienteSearchResultDto>> SearchClientesAsync(
        string termo,
        int estabelecimentoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoEnderecoOptionDto>> GetEnderecosByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoLocalEntregaOptionDto>> GetLocaisEntregaByEnderecoAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task<CotacaoTabelaPrecoOptionDto?> GetTabelaPrecoByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<int?> GetFormaPagamentoByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<string?> GetTipoOVSAPByEnderecoAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOptionDto>> GetTiposOrdemAsync(
        int cotacaoTipoId,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoContratoOptionDto>> GetContratosAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOptionDto>> GetCidadesByUfAsync(
        string cdUf,
        CancellationToken cancellationToken = default);

    Task<CotacaoEditDadosDto?> GetPropostaParaEditAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<CotacaoFreteInicialDto> BuscarFreteInicialAsync(
        int clienteEnderecoId,
        int clienteId,
        string? ufDestino,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOptionDto>> GetFormasPagamentoAsync(
        CancellationToken cancellationToken = default);

    Task<CotacaoEmailTemplateDto?> GetDadosEmailTemplateAsync(
        int propostaId,
        CancellationToken cancellationToken = default);
}
