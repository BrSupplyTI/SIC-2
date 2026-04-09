namespace SIC.Web.Models.PrePedidosPDF;

/// <summary>
/// ViewModel de local de entrega (GetLocaisEntrega).
/// </summary>
public sealed class PrePedidoPDFLocalEntregaViewModel
{
    public int ClienteLocalEntregaID { get; set; }
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string CdControle { get; set; } = string.Empty;
}
