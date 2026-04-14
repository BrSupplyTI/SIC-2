using SIC.Api.Contracts.Admin;

namespace SIC.Api.Services.Admin;

public interface IAdminNoticeService
{
    Task<IReadOnlyList<AdminNoticeDto>> GetAllNoticesAsync(CancellationToken cancellationToken = default);
    Task ExpireNoticeAsync(int avisoId, CancellationToken cancellationToken = default);
    Task DeleteNoticeAsync(int avisoId, CancellationToken cancellationToken = default);
    Task CreateNoticeAsync(CreateNoticeRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IntranetAreaDto>> GetAreasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminUserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
}
