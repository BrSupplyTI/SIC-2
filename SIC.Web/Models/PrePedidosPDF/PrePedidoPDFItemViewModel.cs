namespace SIC.Web.Models.PrePedidosPDF;

/// <summary>
/// ViewModel de um item do pré-pedido (getItens).
/// </summary>
public sealed class PrePedidoPDFItemViewModel
{
    public int PDFPrePedidoPDFItemID { get; set; }
    public int PDFPrePedidoPDFID { get; set; }
    public int PDFSeqItem { get; set; }
    public int PDFQtde { get; set; }
    public int ItemInternoID { get; set; }
    public string ItemCliente { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ItemID { get; set; } = string.Empty;
    public string ItemBrSupply { get; set; } = string.Empty;
    public int SegmentoID { get; set; }
    public int FamiliaID { get; set; }
    public string VlrTblPrecoFormat { get; set; } = string.Empty;
    public string PDFVlrUnit { get; set; } = string.Empty;
    public string VlrTotal { get; set; } = string.Empty;
    public string VlrTotalPedido { get; set; } = string.Empty;
}
