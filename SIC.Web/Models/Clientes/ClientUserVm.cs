namespace SIC.Web.Models.Clientes;

public sealed class ClientUserVm
{
    public int ClienteUsuarioID { get; set; }
    public string Login { get; set; } = string.Empty;
    public string NmUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NmPerfil { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public string Permissao { get; set; } = string.Empty;
    public string Catalogo { get; set; } = string.Empty;
    public string? DtCadastro { get; set; }
    public string? DtUltimoLogin { get; set; }
}
