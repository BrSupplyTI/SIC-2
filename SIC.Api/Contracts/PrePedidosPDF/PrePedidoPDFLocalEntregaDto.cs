namespace SIC.Api.Contracts.PrePedidosPDF;

public sealed class PrePedidoPDFLocalEntregaDto
{
    public int ClienteLocalEntregaID { get; set; }
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string CdControle { get; set; } = string.Empty;
}
