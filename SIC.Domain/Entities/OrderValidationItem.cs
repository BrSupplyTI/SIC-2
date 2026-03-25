namespace SIC.Domain.Entities;

public sealed class OrderValidationItem
{
    public string Erro { get; set; } = string.Empty;
    public string Correcao { get; set; } = string.Empty;
}
