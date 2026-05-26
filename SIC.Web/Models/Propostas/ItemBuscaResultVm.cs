namespace SIC.Web.Models.Propostas;

public sealed class ItemBuscaResultVm
{
    public int ItemID { get; set; }
    public int Probabilidade { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public string Qualidade { get; set; } = string.Empty;
    public string VlrCustoAquisicaoFormat { get; set; } = string.Empty;
}
