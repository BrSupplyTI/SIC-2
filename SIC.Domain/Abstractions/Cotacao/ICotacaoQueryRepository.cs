using SIC.Domain.Entities.Cotacao;

namespace SIC.Domain.Abstractions.Cotacao;

/// <summary>
/// Operações de leitura da Cotação no banco de dados.
/// </summary>
public interface ICotacaoQueryRepository
{
    Task<IReadOnlyList<CotacaoCatalogoItem>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoListItem>> GetListAsync(
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

    Task<CotacaoDetalhe?> GetByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoDetalheItem>> GetItensByPropostaIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOption>> GetEstabelecimentoOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOption>> GetStatusOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOption>> GetCondicoesPagamentoAsync(
        int estabelecimentoId,
        decimal valorTotal,
        CancellationToken cancellationToken = default);

    Task<string> GetExecutivoVendasAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoFreteOpcao>> CalcularFretePropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<CotacaoItemImpostos?> GetImpostosItemAsync(
        int propostaItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoItemValidacao>> ValidarItensImportacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<CotacaoDadosEmail?> GetEnviarEmailDadosAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoEnvioHistoricoItem>> GetHistoricoEnviosAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoTipoOption>> GetTiposCotacaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOption>> GetMotivosBonificacaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoEstabelecimentoOption>> GetEstabelecimentosAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoUfOption>> GetUfsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoClienteSearchResult>> SearchClientesAsync(
        string termo,
        int estabelecimentoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoEnderecoOption>> GetEnderecosByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoLocalEntregaOption>> GetLocaisEntregaByEnderecoAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task<CotacaoTabelaPrecoOption?> GetTabelaPrecoByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<int?> GetFormaPagamentoByClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<string?> GetTipoOVSAPByEnderecoAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOption>> GetTiposOrdemAsync(
        int cotacaoTipoId,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoContratoOption>> GetContratosAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOption>> GetCidadesByUfAsync(
        string cdUf,
        CancellationToken cancellationToken = default);

    Task<CotacaoEditDados?> GetPropostaParaEditAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<(decimal Frete, decimal VlrPedidoMinimo)> BuscarFreteInicialAsync(
        int clienteEnderecoId,
        int clienteId,
        string? ufDestino,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CotacaoSelectOption>> GetFormasPagamentoAsync(
        CancellationToken cancellationToken = default);

    Task<CotacaoEmailTemplate?> GetDadosEmailTemplateAsync(
        int propostaId,
        CancellationToken cancellationToken = default);
}
