namespace SIC.Web.Models.Pedidos;

public sealed class PedidoOcItemViewModel
{
    public int PedidoId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string? DataPedido { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public string OrdemCompra { get; set; } = string.Empty;
    public decimal ValorTotalProdutos { get; set; }
    public string EstabelecimentoNome { get; set; } = string.Empty;
    public string PedidoDetalheUrl { get; set; } = string.Empty;
}
