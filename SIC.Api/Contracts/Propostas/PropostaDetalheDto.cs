namespace SIC.Api.Contracts.Propostas;

public sealed class PropostaDetalheDto
{
    public int PropostaID { get; set; }
    public int EstabelecimentoID { get; set; }
    public string NomeProposta { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public List<QualSegDetalheDto> QualSeg { get; set; } = [];
}

public sealed class QualSegDetalheDto
{
    public int SegmentoID { get; set; }
    public string NmSegmento { get; set; } = string.Empty;
    public string Qualidade { get; set; } = string.Empty;
    public string QualidadeDesc { get; set; } = string.Empty;
}
