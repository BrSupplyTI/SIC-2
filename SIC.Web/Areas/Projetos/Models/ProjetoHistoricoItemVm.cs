namespace SIC.Web.Areas.Projetos.Models;

public sealed class ProjetoHistoricoItemVm
{
    public int ProjetoHistoricoID { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string DsAcao { get; set; } = string.Empty;
    public string? DtAcao { get; set; }
}
