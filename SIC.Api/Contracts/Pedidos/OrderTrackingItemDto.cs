namespace SIC.Api.Contracts.Pedidos;

public sealed class OrderTrackingItemDto
{
    public string? DtEvento { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
}
