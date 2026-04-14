using SIC.Domain.Entities.Admin;

namespace SIC.Domain.Abstractions.Admin;

public interface IAdminNoticeRepository
{
    Task<IReadOnlyList<AdminNotice>> GetAllNoticesAsync(CancellationToken cancellationToken = default);
    Task ExpireNoticeAsync(int avisoId, CancellationToken cancellationToken = default);
    Task DeleteNoticeAsync(int avisoId, CancellationToken cancellationToken = default);
    Task CreateNoticeAsync(string titulo, string descricao, int prioridade, DateTime dataHoraExpiracao, int? intranetAreaId, int? usuarioId, int usuarioResponsavelId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IntranetArea>> GetAreasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminUser>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
}
