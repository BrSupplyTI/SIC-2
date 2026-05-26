namespace SIC.Api.Contracts.Liberacao;

public sealed class LiberacaoPedidoFilterDto
{
    public string? Palavra1 { get; set; }
    public string? Palavra2 { get; set; }
    public string? Palavra3 { get; set; }
    public int FiltroOrdemCompra { get; set; }
    public int FiltroRuptura { get; set; }
    public int FiltroFrete { get; set; }
    public int FiltroMargemNegativa { get; set; }
    public decimal FiltroValorAbaixo { get; set; }
    public decimal FiltroValorAcima { get; set; }
    public string? FiltroIntegracaoSAP { get; set; }
    public string? FiltroContemItem { get; set; }
    public int FiltroAtrasados { get; set; }
    public int FiltroFretePagar { get; set; }
}
