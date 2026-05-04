using SIC.Domain.Entities.Propostas;

namespace SIC.Domain.Abstractions.Propostas;

public interface IPropostaQueryRepository
{
    Task<IReadOnlyList<PropostaListItem>> GetListAsync(
        string? filtroCodigo,
        string? filtroNome,
        string? filtroEstabelecimento,
        string? filtroStatus,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SegmentoItem>> GetSegmentosAsync(
        CancellationToken cancellationToken = default);

    Task<int> SalvarPropostaAsync(
        int estabelecimentoId,
        string nomeProposta,
        CancellationToken cancellationToken = default);

    Task SalvarPropostaQualidadeAsync(
        int propostaId,
        int segmentoId,
        string qualidade,
        CancellationToken cancellationToken = default);

    Task<PropostaDetalhe?> GetByIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task AtualizarPropostaAsync(
        int propostaId,
        int estabelecimentoId,
        string nomeProposta,
        CancellationToken cancellationToken = default);

    Task DeletarPropostaQualidadesAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<PropostaCodificacao?> GetCodificacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemBuscaResult>> BuscarItensBrSupplyAsync(
        int estabelecimentoId,
        string filtro,
        CancellationToken cancellationToken = default);

    Task<bool> AdicionarItemPropostaAsync(
        int propostaId,
        int itemId,
        int qtdAnual,
        decimal margemPadrao,
        string descricaoBreve,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirItemPropostaAsync(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken = default);

    Task<int> ImportarItensAsync(
        int propostaId,
        IReadOnlyList<(string CodCliente, string DescricaoBreve, string DescricaoDetalhada, string Familia, string MarcaFornecedor, string UnidadeMedida, int QtdAnual, decimal Target)> itens,
        CancellationToken cancellationToken = default);

    Task<CodificarItemResult> CodificarItemAsync(
        int propostaItemId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default);

    Task<bool> MarcarSegundoPlanoAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int PropostaID, int EstabelecimentoID)>> GetPropostasPendentesSegundoPlanoAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetItensNaoCodificadosAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task AtualizarStatusPropostaAsync(
        int propostaId,
        int statusId,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<bool> VincularItemManualAsync(
        int propostaItemId,
        int itemId,
        CancellationToken cancellationToken = default);
}
