namespace SIC.Domain.Entities;

/// <summary>
/// Shape retornado por SIC_ProjetoTarefasListar.
/// A montagem hierárquica (SubTarefas) é feita na camada de serviço.
/// </summary>
public sealed class ProjetoTarefa
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
    public DateTime? DtInicio { get; set; }
    public DateTime? DtPrevisaoFim { get; set; }
    public DateTime? DtFimReal { get; set; }
    public int NrOrdem { get; set; }
    public int? ProjetoTarefaPaiID { get; set; }
}
