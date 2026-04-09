namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Entidade de local de entrega do cliente (GetLocaisEntrega).
/// </summary>
public sealed class PrePedidoPDFLocalEntrega
{
    public int ClienteLocalEntregaID { get; set; }
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string CdControle { get; set; } = string.Empty;
}
