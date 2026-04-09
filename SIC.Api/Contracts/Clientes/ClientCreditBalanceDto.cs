namespace SIC.Api.Contracts.Clientes;

public sealed class ClientCreditBalanceDto
{
    public decimal VlrCreditos { get; set; }
    public decimal VlrTitulosEmAberto { get; set; }
    public decimal VlrPedidosNaoFaturados { get; set; }
}
