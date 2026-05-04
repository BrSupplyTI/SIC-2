namespace SIC.Web.Models.Propostas;

public sealed class PropostaDetalheVm
{
    public int PropostaID { get; set; }
    public int EstabelecimentoID { get; set; }
    public string NomeProposta { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public List<QualSegDetalheVm> QualSeg { get; set; } = [];
}

public sealed class QualSegDetalheVm
{
    public int SegmentoID { get; set; }
    public string NmSegmento { get; set; } = string.Empty;
    public string Qualidade { get; set; } = string.Empty;
    public string QualidadeDesc { get; set; } = string.Empty;
}
