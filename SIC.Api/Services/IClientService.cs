using SIC.Api.Contracts.Clientes;
using SIC.Api.Contracts.Produtos;

namespace SIC.Api.Services;

public interface IClientService
{
    Task<ClientSearchResultDto> SearchAsync(ClientSearchFilterDto filter, CancellationToken cancellationToken = default);
    Task<ClientDetailDto?> GetDetailAsync(int clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientWalletDto>> GetWalletsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogEstablishmentDto>> GetEstablishmentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientConsultantDto>> GetConsultantsAsync(int clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientTitleDto>> GetTitulosAsync(int clienteId, CancellationToken cancellationToken = default);
}
