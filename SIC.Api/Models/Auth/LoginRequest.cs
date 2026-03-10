namespace SIC.Api.Models.Auth;

public sealed class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
}
