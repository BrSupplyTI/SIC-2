namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Representa um item retornado por BR_SP_ValidaItensProposta,
/// usado para cruzar com os dados do Excel na importação.
/// </summary>
public sealed class CotacaoItemValidacaoViewModel
{
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public decimal VlrUnit { get; set; }
    public decimal VlrPrecoMinimo { get; set; }
    public decimal VlrCustoAquisicao { get; set; }
    public decimal VlrCustoMedio { get; set; }
}
