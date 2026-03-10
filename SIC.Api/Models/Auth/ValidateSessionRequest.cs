namespace SIC.Api.Models.Auth;

public sealed class ValidateSessionRequest
{
    public int UsuarioId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
    public string? UserAgent { get; set; }
}
