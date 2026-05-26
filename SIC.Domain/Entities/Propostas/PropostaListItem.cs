namespace SIC.Domain.Entities.Propostas;

public sealed class PropostaListItem
{
    public int PropostaID { get; set; }
    public string NomeProposta { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string DtCriacao { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public int TotalItens { get; set; }
    public int ItensProcessados { get; set; }
    public string PercentualConcluido { get; set; } = string.Empty;
}
