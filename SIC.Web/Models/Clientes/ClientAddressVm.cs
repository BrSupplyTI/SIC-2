namespace SIC.Web.Models.Clientes;

public sealed class ClientAddressVm
{
    public int ClienteEnderecoID { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public string CodSAP { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string CPFCNPJ { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string NmCidade { get; set; } = string.Empty;
    public string CdUF { get; set; } = string.Empty;
    public string TabelaPreco { get; set; } = string.Empty;
    public decimal VlrPedidoMinimo { get; set; }
    public decimal VlrTaxaEntrega { get; set; }
}
