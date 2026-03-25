namespace SIC.Web.Models.Pedidos;

public sealed class OrderInvoiceItemVm
{
    public int NotaFiscalID { get; set; }
    public string NrNotaFiscal { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string Operacao { get; set; } = string.Empty;
    public string EmitCNPJ { get; set; } = string.Empty;
    public DateTime? DtEmissao { get; set; }
    public string Versao { get; set; } = string.Empty;
    public int QtdeVolumes { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal VlrTotalNF { get; set; }
    public string StatusNF { get; set; } = string.Empty;
    public string MotivoCancelamento { get; set; } = string.Empty;
    public string DsStatusCancelamento { get; set; } = string.Empty;
    public string CubagemNF { get; set; } = string.Empty;
    public int TipoAtestoID { get; set; }
    public string DsAtestoRecebimento { get; set; } = string.Empty;
}
