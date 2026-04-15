namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoTarefaDto
{
    public int ProjetoTarefaID { get; set; }
    public int ProjetoID { get; set; }
    public string NmTarefa { get; set; } = string.Empty;
    public string? DsTarefa { get; set; }
    public int ProjetoTarefaStatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public string CdCorStatus { get; set; } = string.Empty;
    public int ProjetoTarefaPrioridadeID { get; set; }
    public string NmPrioridade { get; set; } = string.Empty;
    public string CdCorPrioridade { get; set; } = string.Empty;
    public int? UsuarioResponsavelID { get; set; }
    public string NmResponsavel { get; set; } = string.Empty;
    public string? DtInicio { get; set; }
    public string? DtPrevisaoFim { get; set; }
    public string? DtFimReal { get; set; }
    public int NrOrdem { get; set; }
    public int? ProjetoTarefaPaiID { get; set; }
}
