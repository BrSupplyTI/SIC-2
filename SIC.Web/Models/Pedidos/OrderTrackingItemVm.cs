namespace SIC.Web.Models.Pedidos;

public sealed class OrderTrackingItemVm
{
    public string? DtEvento { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
}
