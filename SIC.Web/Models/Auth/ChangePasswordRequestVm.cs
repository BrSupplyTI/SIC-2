namespace SIC.Web.Models.Auth;

public sealed class ChangePasswordRequestVm
{
    public int UsuarioId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
