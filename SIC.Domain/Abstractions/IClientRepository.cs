namespace SIC.Domain.Abstractions;

using SIC.Domain.Entities;

public interface IClientRepository
{
    Task<IReadOnlyList<ClientSearchItem>> SearchAsync(
        int pageNumber,
        int pageSize,
        string? contemTexto,
        string? comecaComTexto,
        int flagAtivo,
        int estabelecimentoId,
        int flagClienteMae,
        int carteiraId,
        int qtDiasUltimoPedido,
        string? orderBy,
        int usuarioId,
        CancellationToken cancellationToken = default);

    Task<ClientDetail?> GetDetailAsync(int clienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientWallet>> GetWalletsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogEstablishment>> GetEstablishmentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientConsultant>> GetConsultantsAsync(int clienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientTitle>> GetTitulosAsync(int clienteId, CancellationToken cancellationToken = default);

    Task<ClientCreditBalance> GetCreditBalanceAsync(int clienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientAddress>> GetAddressesAsync(int clienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientDeliveryLocation>> GetDeliveryLocationsAsync(int clienteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientUser>> GetUsersAsync(int clienteId, CancellationToken cancellationToken = default);
}
