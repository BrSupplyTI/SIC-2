using SIC.Domain.Entities;

namespace SIC.Domain.Abstractions;

public interface IAuthRepository
{
    Task<AuthUser?> GetUserByPasswordAsync(string login, string password, string? masterPassword, CancellationToken cancellationToken = default);
    Task<AuthUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthUser?> GetUserByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);

    Task<string?> GetActiveEmailByUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken = default);

    Task<bool> TryCreateSessionAsync(int usuarioId, string sessionToken, int timeoutMinutes, string remoteIp, string? userAgent, CancellationToken cancellationToken = default);
    Task<bool> RefreshSessionAsync(int usuarioId, string sessionToken, int timeoutMinutes, string remoteIp, string? userAgent, CancellationToken cancellationToken = default);
    Task<bool> DeactivateSessionAsync(int usuarioId, string sessionToken, CancellationToken cancellationToken = default);
    Task CleanupExpiredSessionsAsync(int usuarioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthEstablishment>> GetAuthorizedEstablishmentsAsync(int usuarioId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> IsAuthorizedForEstablishmentAsync(int usuarioId, bool isAdmin, int estabelecimentoId, CancellationToken cancellationToken = default);
    Task<bool> ChangeEstablishmentAsync(int usuarioId, int estabelecimentoId, CancellationToken cancellationToken = default);

    Task<bool> UpdateUserPhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(int usuarioId, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(int usuarioId, string newPassword, string? updatePasswordSql, CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task InsertLogAsync(string message, int usuarioId, CancellationToken cancellationToken = default);
}
