namespace SIC.Domain.Entities;

/// <summary>
/// Shape retornado por SIC_ProjetosListar (lista paginada).
/// </summary>
public sealed class ProjetoListItem
{
    public int ProjetoID { get; set; }
    public string NmProjeto { get; set; } = string.Empty;
    public string DsProjeto { get; set; } = string.Empty;
    public int ProjetoStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCorStatus { get; set; } = string.Empty;
    public DateTime? DtInicio { get; set; }
    public DateTime? DtPrevisaoFim { get; set; }
    public DateTime? DtFimReal { get; set; }
    public int UsuarioCriadorID { get; set; }
    public string NmCriador { get; set; } = string.Empty;
    public DateTime DtCriacao { get; set; }
    public int QtTarefas { get; set; }
    public int QtTarefasConcluidas { get; set; }
    public int QtParticipantes { get; set; }
    public int TotalRegistros { get; set; }
}
