namespace SIC.Api.Models.Auth;

public sealed class ChangePasswordRequest
{
    public int UsuarioId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
