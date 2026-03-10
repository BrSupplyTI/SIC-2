namespace SIC.Web.Models.Auth;

public sealed class SsoLoginRequestVm
{
    public string Email { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
}
