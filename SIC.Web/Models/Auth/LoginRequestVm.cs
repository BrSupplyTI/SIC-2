namespace SIC.Web.Models.Auth;

public sealed class LoginRequestVm
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RemoteIp { get; set; }
}
