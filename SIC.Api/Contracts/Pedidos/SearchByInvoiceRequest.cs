namespace SIC.Api.Contracts.Pedidos;

public sealed class SearchByInvoiceRequest
{
    public string? NotaFiscal { get; set; }
    public int? Serie { get; set; }
}
