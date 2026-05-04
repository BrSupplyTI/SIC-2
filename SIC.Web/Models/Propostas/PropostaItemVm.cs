namespace SIC.Web.Models.Propostas;

public sealed class PropostaItemVm
{
    public int PropostaID { get; set; }
    public string NomeProposta { get; set; } = string.Empty;
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string DtCriacao { get; set; } = string.Empty;
    public string NmStatus { get; set; } = string.Empty;
    public string PercentualConcluido { get; set; } = string.Empty;
}
