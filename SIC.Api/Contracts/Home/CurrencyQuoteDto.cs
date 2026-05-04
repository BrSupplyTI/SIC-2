namespace SIC.Api.Contracts.Home;

public sealed class CurrencyQuoteDto
{
    public string Moeda { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Variacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
