namespace SIC.Api.Contracts.Clientes;

public sealed class ClientTitleDto
{
    public string? DtEmissao { get; set; }
    public string NrNotaFiscal { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Pedido { get; set; } = string.Empty;
    public string Parcela { get; set; } = string.Empty;
    public string? DtVencimento { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public decimal VlrOriginal { get; set; }
    public decimal VlrSaldo { get; set; }
}
