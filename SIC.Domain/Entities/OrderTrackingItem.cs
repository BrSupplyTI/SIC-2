namespace SIC.Domain.Entities;

public sealed class OrderTrackingItem
{
    public DateTime? DtEvento { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
}
