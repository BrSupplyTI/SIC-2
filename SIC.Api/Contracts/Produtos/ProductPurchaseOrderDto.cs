namespace SIC.Api.Contracts.Produtos;

public sealed class ProductPurchaseOrderDto
{
    public int Quantidade { get; set; }
    public string? DtPrevisao { get; set; }
    public string OrdemCompra { get; set; } = string.Empty;
    public string XPed { get; set; } = string.Empty;
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string CdEstabelecimento { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
}
