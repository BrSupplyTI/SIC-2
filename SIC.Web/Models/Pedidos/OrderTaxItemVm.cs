namespace SIC.Web.Models.Pedidos;

public sealed class OrderTaxItemVm
{
    public decimal? MVA { get; set; }
    public decimal? VlrTotalNF { get; set; }
    public string ItemDocumentoSAP { get; set; } = string.Empty;
    public string CdItem { get; set; } = string.Empty;
    public decimal? MKUP { get; set; }
    public decimal? VlrUnitario { get; set; }
    public decimal? VlrCustoAquisicao { get; set; }
    public decimal? MargemEnviada { get; set; }
    public decimal? PercentualICMS { get; set; }
    public decimal? PercentualFCP { get; set; }
    public decimal? PercentualIPI { get; set; }
    public decimal? PercentualCOFINS { get; set; }
    public decimal? PercentualPIS { get; set; }
    public decimal? ValorICMS { get; set; }
    public decimal? ValorIPI { get; set; }
    public decimal? ValorST { get; set; }
    public decimal? ValorISS { get; set; }
    public decimal? ValorISSRetido { get; set; }
    public decimal? ValorCOFINS { get; set; }
    public decimal? ValorPIS { get; set; }
    public decimal? ValorFCPST { get; set; }
    public decimal? ValorICMSPartilhaOrigem { get; set; }
    public decimal? ValorICMSPartilhaDestino { get; set; }
    public decimal? ValorFundoCombPobreza { get; set; }
    public decimal? ValorPISRetido { get; set; }
    public decimal? ValorCOFINSRetido { get; set; }
    public decimal? ValorCSLRetido { get; set; }
    public decimal? ValorIRRetido { get; set; }
    public decimal? MargemCalculada { get; set; }
    public decimal? LB { get; set; }
    public decimal? ROL { get; set; }
}
