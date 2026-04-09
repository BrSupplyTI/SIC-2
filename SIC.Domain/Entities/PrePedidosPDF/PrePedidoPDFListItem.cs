namespace SIC.Domain.Entities.PrePedidosPDF;

public sealed class PrePedidoPDFListItem
{
    public int PDFPrePedidoPDFID { get; set; }
    public int ClienteID { get; set; }
    public string NmCliente { get; set; } = string.Empty;
    public string OrdemCompra { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public int CotacaoID { get; set; }
    public int StatusPrePedidoPDFID { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public string CriadoEm { get; set; } = string.Empty;
}
