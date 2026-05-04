namespace SIC.Domain.Entities.Propostas;

public sealed class PropostaDetalhe
{
    public int PropostaID { get; set; }
    public int EstabelecimentoID { get; set; }
    public string NomeProposta { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public List<PropostaQualSegItem> QualSeg { get; set; } = [];
}

public sealed class PropostaQualSegItem
{
    public int SegmentoID { get; set; }
    public string NmSegmento { get; set; } = string.Empty;
    public string Qualidade { get; set; } = string.Empty;
}
