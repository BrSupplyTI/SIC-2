namespace SIC.Web.Models.Home;

public sealed class CurrencyQuoteVm
{
    public string Moeda { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Variacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
