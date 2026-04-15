namespace SIC.Domain.Entities;

/// <summary>
/// Shape retornado por SIC_ProjetoParticipantesListar.
/// </summary>
public sealed class ProjetoParticipante
{
    public int ProjetoParticipanteID { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string NmPapel { get; set; } = string.Empty;
    public DateTime DtEntrada { get; set; }
}
