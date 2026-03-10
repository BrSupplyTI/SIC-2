namespace SIC.Api.Models.Auth;

public sealed class LogoutSessionRequest
{
    public int UsuarioId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
}
