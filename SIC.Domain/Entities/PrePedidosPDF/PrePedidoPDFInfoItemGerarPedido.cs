namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Dados de um item para gerar a cotação (BR_sp_InsertCotacaoItem).
/// Equivalente ao GetInfoItensGerarPedido do PHP.
/// </summary>
public sealed class PrePedidoPDFInfoItemGerarPedido
{
    public int CotacaoID { get; set; }
    public int Tipo { get; set; }
    public int ItemID { get; set; }
    public int QtItem { get; set; }
    public decimal VlrUnit { get; set; }
    public string CdItemCliente { get; set; } = string.Empty;
    public string OrdemCliente { get; set; } = string.Empty;
    public int SeqCliente { get; set; }
}
