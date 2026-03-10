using SIC.Api.Models.Auth;
using SIC.Api.Models.Profile;

namespace SIC.Api.Services;

public interface IUserProfileService
{
    Task<IReadOnlyList<AreaOptionDto>> GetAreasAsync(CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetProfileAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateProfileAsync(int usuarioId, int? areaId, string? telefone, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdatePhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default);
    Task<OperationResult> RemovePhotoAsync(int usuarioId, CancellationToken cancellationToken = default);
}
