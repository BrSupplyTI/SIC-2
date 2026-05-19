namespace SIC.Api.Contracts.Cotacao;

public sealed class CotacaoItemValidacaoDto
{
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public decimal VlrUnit { get; set; }
    public decimal VlrPrecoMinimo { get; set; }
    public decimal VlrCustoAquisicao { get; set; }
    public decimal VlrCustoMedio { get; set; }
}
