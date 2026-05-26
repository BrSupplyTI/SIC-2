using SIC.Api.Contracts.Propostas;

namespace SIC.Api.Services.Propostas;

public interface IPropostaQueryService
{
    Task<IReadOnlyList<PropostaListItemDto>> GetListAsync(
        string? filtroCodigo,
        string? filtroNome,
        string? filtroEstabelecimento,
        string? filtroStatus,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SegmentoItemDto>> GetSegmentosAsync(
        CancellationToken cancellationToken = default);

    Task<SalvarPropostaResponse> SalvarPropostaAsync(
        SalvarPropostaRequest request,
        CancellationToken cancellationToken = default);

    Task<PropostaDetalheDto?> GetByIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<PropostaCodificacaoDto?> GetCodificacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemBuscaResultDto>> BuscarItensBrSupplyAsync(
        int estabelecimentoId,
        string filtro,
        CancellationToken cancellationToken = default);

    Task<bool> AdicionarItemPropostaAsync(
        AdicionarItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirItemPropostaAsync(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken = default);

    Task<int> ImportarItensAsync(
        ImportarItensRequest request,
        CancellationToken cancellationToken = default);

    Task<CodificarItemResultDto> CodificarItemAsync(
        int propostaItemId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default);

    Task<bool> MarcarSegundoPlanoAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default);

    Task<bool> VincularItemManualAsync(
        int propostaItemId,
        int itemId,
        CancellationToken cancellationToken = default);
}
