namespace SIC.Web.Models.PrePedidosPDF;

/// <summary>
/// ViewModel de CNPJ do cliente (GetListCNPJCliente).
/// </summary>
public sealed class PrePedidoPDFCnpjViewModel
{
    public int ClienteEnderecoID { get; set; }
    public string CPFCNPJ { get; set; } = string.Empty;
}
