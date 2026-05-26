namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoListItemDto
{
    public int ProjetoID { get; set; }
    public string NmProjeto { get; set; } = string.Empty;
    public string DsProjeto { get; set; } = string.Empty;
    public int ProjetoStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCorStatus { get; set; } = string.Empty;
    public string? DtInicio { get; set; }
    public string? DtPrevisaoFim { get; set; }
    public string? DtFimReal { get; set; }
    public int UsuarioCriadorID { get; set; }
    public string NmCriador { get; set; } = string.Empty;
    public string DtCriacao { get; set; } = string.Empty;
    public int QtTarefas { get; set; }
    public int QtTarefasConcluidas { get; set; }
    public int QtParticipantes { get; set; }
}
