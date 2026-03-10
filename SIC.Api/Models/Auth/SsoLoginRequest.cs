namespace SIC.Api.Models.Auth;

public sealed class SsoLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
}
