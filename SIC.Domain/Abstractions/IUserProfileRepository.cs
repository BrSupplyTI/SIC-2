using SIC.Domain.Entities;

namespace SIC.Domain.Abstractions;

public interface IUserProfileRepository
{
    Task<IReadOnlyList<AreaOption>> GetAreasAsync(CancellationToken cancellationToken = default);
    Task<UserProfile?> GetProfileAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserPermission>> GetPermissionsAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfileAsync(int usuarioId, int? areaId, string? telefone, string? ramal, int? matricula, string? cargo, string? setor, int? diaAniversario, int? mesAniversario, CancellationToken cancellationToken = default);
    Task<bool> UpdatePhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default);
    Task<bool> RemovePhotoAsync(int usuarioId, CancellationToken cancellationToken = default);
}
