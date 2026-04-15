namespace SIC.Domain.Entities;

public sealed class CurrencyQuote
{
    public string Moeda { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Variacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
