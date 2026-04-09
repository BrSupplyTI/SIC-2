namespace SIC.Api.Contracts.PrePedidosPDF;

public sealed class PrePedidoPDFCnpjDto
{
    public int ClienteEnderecoID { get; set; }
    public string CPFCNPJ { get; set; } = string.Empty;
}
