namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Entidade de CNPJ do cliente (GetListCNPJCliente).
/// </summary>
public sealed class PrePedidoPDFCnpj
{
    public int ClienteEnderecoID { get; set; }
    public string CPFCNPJ { get; set; } = string.Empty;
}
