namespace SIC.Api.Contracts.Projetos;

public sealed class ParticipanteAtualizarPapelDto
{
    public int ProjetoParticipanteID { get; set; }
    public string NmPapel { get; set; } = string.Empty;
    public int UsuarioLogadoID { get; set; }
}
