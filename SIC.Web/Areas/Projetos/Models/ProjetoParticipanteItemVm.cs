namespace SIC.Web.Areas.Projetos.Models;

public sealed class ProjetoParticipanteItemVm
{
    public int ProjetoParticipanteID { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string NmPapel { get; set; } = string.Empty;
    public string? DtEntrada { get; set; }
}
