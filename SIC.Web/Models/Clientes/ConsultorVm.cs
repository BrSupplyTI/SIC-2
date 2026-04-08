namespace SIC.Web.Models.Clientes;

public sealed class ConsultorVm
{
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
}
