namespace SIC.Web.Models.PrePedidosPDF;

/// <summary>
/// ViewModel de candidato para troca de item (TrocarItem).
/// </summary>
public sealed class PrePedidoPDFTrocaItemViewModel
{
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int ItemID { get; set; }
    public decimal VlrTabelaPreco { get; set; }
}
