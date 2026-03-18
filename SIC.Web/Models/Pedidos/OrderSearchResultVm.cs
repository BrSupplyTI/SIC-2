namespace SIC.Web.Models.Pedidos;

public sealed class OrderSearchResultVm
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? TotalPedidos { get; set; }
    public bool ShowModal { get; set; }
    public string? RedirectUrl { get; set; }
    public IReadOnlyList<PedidoOcItemViewModel> Pedidos { get; set; } = [];
}
