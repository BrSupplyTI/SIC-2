namespace SIC.Api.Contracts.Produtos;

public sealed class ProductStockAllocationDto
{
    public int Pedido { get; set; }
    public string DtPedido { get; set; } = string.Empty;
    public string? DtProgLiberacao { get; set; }
    public string NmCliente { get; set; } = string.Empty;
    public string DsStatusCotacao { get; set; } = string.Empty;
    public string CdEstabelecimento { get; set; } = string.Empty;
    public int QtSolicitada { get; set; }
    public int QtRupturas { get; set; }
    public string NmCanalVenda { get; set; } = string.Empty;
    public string OrdemVendaSAP { get; set; } = string.Empty;
}
