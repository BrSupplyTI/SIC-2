namespace SIC.Web.Models.Pedidos;

public sealed class PedidoBuscaViewModel
{
    public string? InputPedido { get; set; }
    public string? InputOrdemCompra { get; set; }
    public string? InputNotaFiscal { get; set; }
    public int? InputSerieNF { get; set; }

    public string? ErroPedido { get; set; }
    public string? ErroOrdemCompra { get; set; }
    public string? ErroNotaFiscal { get; set; }

    public bool ShowOcModal { get; set; }
    public int TotalOcPedidos { get; set; }
    public IReadOnlyList<PedidoOcItemViewModel> OcPedidos { get; set; } = [];
}
