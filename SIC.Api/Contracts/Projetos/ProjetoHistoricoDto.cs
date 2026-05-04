namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoHistoricoDto
{
    public int ProjetoHistoricoID { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string DsAcao { get; set; } = string.Empty;
    public string DtAcao { get; set; } = string.Empty;
}
