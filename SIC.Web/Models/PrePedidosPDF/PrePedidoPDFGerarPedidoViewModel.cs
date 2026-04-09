namespace SIC.Web.Models.PrePedidosPDF;

/// <summary>
/// ViewModel com dados necessários para gerar pedido (GetInfoGerarPedido).
/// </summary>
public sealed class PrePedidoPDFGerarPedidoViewModel
{
    public int EstabelecimentoID { get; set; }
    public int ClienteID { get; set; }
    public int ClienteEnderecoID { get; set; }
    public string CNPJ { get; set; } = string.Empty;
    public int ClienteLocalEntregaID { get; set; }
    public int ClienteUsuarioID { get; set; }
    public int NaturezaOperacaoID { get; set; }
    public int CondPagtoID { get; set; }
    public string OrdemCompra { get; set; } = string.Empty;
    public int? ClienteCategoriaPedidoID { get; set; }
}
