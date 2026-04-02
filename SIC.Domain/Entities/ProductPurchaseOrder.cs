namespace SIC.Domain.Entities;

public sealed class ProductPurchaseOrder
{
    public int Quantidade { get; set; }
    public DateTime? DtPrevisao { get; set; }
    public string OrdemCompra { get; set; } = string.Empty;
    public string XPed { get; set; } = string.Empty;
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string CdEstabelecimento { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
}
