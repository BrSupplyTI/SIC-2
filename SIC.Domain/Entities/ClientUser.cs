namespace SIC.Domain.Entities;

public sealed class ClientUser
{
    public int ClienteUsuarioID { get; set; }
    public string Login { get; set; } = string.Empty;
    public string NmUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NmPerfil { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public string Permissao { get; set; } = string.Empty;
    public string Catalogo { get; set; } = string.Empty;
    public DateTime? DtCadastro { get; set; }
    public DateTime? DtUltimoLogin { get; set; }
}
