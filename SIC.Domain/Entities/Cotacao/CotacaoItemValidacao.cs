namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Item retornado por BR_SP_ValidaItensProposta (importação de cotação via Excel).
/// </summary>
public sealed class CotacaoItemValidacao
{
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public decimal VlrUnit { get; set; }
    public decimal VlrPrecoMinimo { get; set; }
    public decimal VlrCustoAquisicao { get; set; }
    public decimal VlrCustoMedio { get; set; }
}
