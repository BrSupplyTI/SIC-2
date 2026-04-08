namespace SIC.Domain.Entities;

public sealed class ClientConsultant
{
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
}
