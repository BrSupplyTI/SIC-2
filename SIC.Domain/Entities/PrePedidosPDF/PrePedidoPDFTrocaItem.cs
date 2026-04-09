namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Entidade de item para troca (GetTrocaItens).
/// </summary>
public sealed class PrePedidoPDFTrocaItem
{
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int ItemID { get; set; }
    public decimal VlrTabelaPreco { get; set; }
}
