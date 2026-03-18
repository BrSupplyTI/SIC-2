namespace SIC.Domain.Entities;

public sealed class PurchaseOrderOrderItem
{
    public int PedidoId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public DateTime? DataPedido { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public decimal ValorTotalProdutos { get; set; }
    public string EstabelecimentoNome { get; set; } = string.Empty;
    public string OrdemCompra { get; set; }
}
