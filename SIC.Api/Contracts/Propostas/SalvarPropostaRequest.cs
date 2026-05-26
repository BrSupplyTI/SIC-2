namespace SIC.Api.Contracts.Propostas;

public sealed class SalvarPropostaRequest
{
    public int? PropostaID { get; set; }
    public int EstabelecimentoID { get; set; }
    public string NomeProposta { get; set; } = string.Empty;
    public List<QualSegItem> QualSeg { get; set; } = [];
}

public sealed class QualSegItem
{
    public int SegmentoID { get; set; }
    public string Qualidade { get; set; } = string.Empty;
}
