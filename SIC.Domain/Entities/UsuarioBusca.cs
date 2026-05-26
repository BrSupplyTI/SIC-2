namespace SIC.Domain.Entities;

/// <summary>
/// Shape retornado por SIC_ProjetoUsuariosBuscar (autocomplete de participantes).
/// </summary>
public sealed class UsuarioBusca
{
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
}
