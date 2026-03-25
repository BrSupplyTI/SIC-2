namespace SIC.Web.Models.Pedidos;

public sealed class OrderBrSupplyItemVm
{
    public int ClienteID { get; set; }
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int QtItem { get; set; }
    public decimal VlrFinal { get; set; }
    public decimal VlrTotal { get; set; }
    public decimal VlrOriginal { get; set; }
    public string OrdemCliente { get; set; } = string.Empty;
    public string SituacaoItem { get; set; } = string.Empty;
    public string? DtAlocacao { get; set; }
    public decimal? MargemCalculada { get; set; }
    public string Versao { get; set; } = string.Empty;
    public string Foto { get; set; } = string.Empty;
    public string PathFoto { get; set; } = string.Empty;
    public string MensagemRuptura { get; set; } = string.Empty;
    public string? DtPrevEntrega { get; set; }
    public int? QtDisponivel { get; set; }
    public int? QtItemPrevEntrega { get; set; }
    public string NmFornecedor { get; set; } = string.Empty;
}
