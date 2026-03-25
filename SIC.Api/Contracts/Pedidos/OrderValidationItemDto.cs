namespace SIC.Api.Contracts.Pedidos;

public sealed class OrderValidationItemDto
{
    public string Erro { get; set; } = string.Empty;
    public string Correcao { get; set; } = string.Empty;
}
