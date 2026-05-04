namespace SIC.Web.Models.Propostas;

public sealed class SalvarPropostaRequestVm
{
    public int? PropostaID { get; set; }
    public int EstabelecimentoID { get; set; }
    public string NomeProposta { get; set; } = string.Empty;
    public List<QualSegItemVm> QualSeg { get; set; } = [];
}

public sealed class QualSegItemVm
{
    public int SegmentoID { get; set; }
    public string Qualidade { get; set; } = string.Empty;
}
