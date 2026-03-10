namespace SIC.Web.Models.Auth;

public sealed class ResetPasswordRequestVm
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
