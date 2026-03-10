using SIC.Api.Models.Auth;

namespace SIC.Api.Services;

public interface ISicAuthService
{
    Task<AuthResult> LoginWithPasswordAsync(string login, string password, string remoteIp, string? userAgent, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginWithSsoAsync(string email, string remoteIp, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResult> ValidateSessionAsync(int usuarioId, string sessionToken, string remoteIp, string? userAgent, CancellationToken cancellationToken = default);
    Task<OperationResult> LogoutSessionAsync(int usuarioId, string sessionToken, CancellationToken cancellationToken = default);
    Task<OperationResult> RequestPasswordResetAsync(string identifier, string remoteIp, CancellationToken cancellationToken = default);
    Task<OperationResult> ResetPasswordAsync(string token, string newPassword, string remoteIp, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EstablishmentDto>> GetAuthorizedEstablishmentsAsync(int usuarioId, bool isAdmin, int? currentEstabelecimentoId, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangeEstablishmentAsync(int usuarioId, bool isAdmin, int estabelecimentoId, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateUserPhotoAsync(int usuarioId, string foto, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangePasswordAsync(int usuarioId, string newPassword, CancellationToken cancellationToken = default);
}
