namespace SIC.Api.Contracts.Clientes;

public sealed class ClientDeliveryLocationDto
{
    public int ClienteLocalEntregaID { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public string CdControle { get; set; } = string.Empty;
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string CPFCNPJ { get; set; } = string.Empty;
    public string NmCidade { get; set; } = string.Empty;
    public string CdUF { get; set; } = string.Empty;
    public string NmCanalVenda { get; set; } = string.Empty;
    public string SituacaoCredito { get; set; } = string.Empty;
    public string TipoEndereco { get; set; } = string.Empty;    
}
