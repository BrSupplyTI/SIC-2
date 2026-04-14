using SIC.Api.Contracts.Admin;
using SIC.Domain.Abstractions.Admin;

namespace SIC.Api.Services.Admin;

public sealed class AdminNoticeService(IAdminNoticeRepository repository) : IAdminNoticeService
{
    public async Task<IReadOnlyList<AdminNoticeDto>> GetAllNoticesAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetAllNoticesAsync(cancellationToken);

        return items.Select(i => new AdminNoticeDto
        {
            AvisoID = i.AvisoID,
            Titulo = i.Titulo,
            Descricao = i.Descricao,
            Prioridade = i.Prioridade,
            DataHoraEnvio = i.DataHoraEnvio,
            DataHoraExpiracao = i.DataHoraExpiracao,
            Responsavel = i.Responsavel,
            Destinatario = i.Destinatario,
            Situacao = i.Situacao,
            QtLeituras = i.QtLeituras
        }).ToList();
    }

    public async Task ExpireNoticeAsync(int avisoId, CancellationToken cancellationToken = default)
        => await repository.ExpireNoticeAsync(avisoId, cancellationToken);

    public async Task DeleteNoticeAsync(int avisoId, CancellationToken cancellationToken = default)
        => await repository.DeleteNoticeAsync(avisoId, cancellationToken);

    public async Task CreateNoticeAsync(CreateNoticeRequest request, CancellationToken cancellationToken = default)
    {
        await repository.CreateNoticeAsync(
            request.Titulo,
            request.Descricao,
            request.Prioridade,
            request.DataHoraExpiracao,
            request.IntranetAreaID,
            request.UsuarioID,
            request.UsuarioResponsavelID,
            cancellationToken);
    }

    public async Task<IReadOnlyList<IntranetAreaDto>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetAreasAsync(cancellationToken);
        return items.Select(i => new IntranetAreaDto
        {
            IntranetAreaID = i.IntranetAreaID,
            NmArea = i.NmArea
        }).ToList();
    }

    public async Task<IReadOnlyList<AdminUserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetActiveUsersAsync(cancellationToken);
        return items.Select(i => new AdminUserDto
        {
            UsuarioID = i.UsuarioID,
            NmUsuario = i.NmUsuario
        }).ToList();
    }
}
