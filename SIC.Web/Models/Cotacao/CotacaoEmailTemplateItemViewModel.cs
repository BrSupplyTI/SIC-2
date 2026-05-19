namespace SIC.Web.Models.Cotacao;

public sealed class CotacaoEmailTemplateItemViewModel
{
    public string CodItemBR { get; set; } = string.Empty;
    public string DescrItemBR { get; set; } = string.Empty;
    public decimal PrecoItem { get; set; }
    public decimal IPI { get; set; }
    public decimal ST { get; set; }
    public decimal Quantidade { get; set; }
    public decimal VlrUnitario { get; set; }
    public string NmSegmento { get; set; } = string.Empty;
    public string NCM { get; set; } = string.Empty;
}
