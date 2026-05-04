namespace SIC.Api.Contracts.Projetos;

public sealed class ParticipanteAdicionarDto
{
    public int ProjetoID { get; set; }
    public int UsuarioID { get; set; }
    public string NmPapel { get; set; } = string.Empty;
    public int UsuarioLogadoID { get; set; }
}
