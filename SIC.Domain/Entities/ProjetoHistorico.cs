namespace SIC.Domain.Entities;

/// <summary>
/// Shape retornado por SIC_ProjetoHistoricoListar.
/// </summary>
public sealed class ProjetoHistorico
{
    public int ProjetoHistoricoID { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string DsAcao { get; set; } = string.Empty;
    public DateTime DtAcao { get; set; }
}
