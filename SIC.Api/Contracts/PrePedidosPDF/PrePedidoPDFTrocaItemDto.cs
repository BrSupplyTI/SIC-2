namespace SIC.Api.Contracts.PrePedidosPDF;

public sealed class PrePedidoPDFTrocaItemDto
{
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int ItemID { get; set; }
    public decimal VlrTabelaPreco { get; set; }
}
