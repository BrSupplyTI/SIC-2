namespace SIC.Domain.Entities;

public sealed class ClientTitle
{
    public DateTime? DtEmissao { get; set; }
    public string NrNotaFiscal { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Parcela { get; set; } = string.Empty;
    public DateTime? DtVencimento { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public decimal VlrOriginal { get; set; }
    public decimal VlrSaldo { get; set; }
}
