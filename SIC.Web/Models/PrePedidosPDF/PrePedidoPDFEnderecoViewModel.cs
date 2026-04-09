namespace SIC.Web.Models.PrePedidosPDF;

/// <summary>
/// ViewModel de endereço do cliente (GetEnderecos).
/// </summary>
public sealed class PrePedidoPDFEnderecoViewModel
{
    public int ClienteEnderecoID { get; set; }
    public string Logradouro { get; set; } = string.Empty;
}
