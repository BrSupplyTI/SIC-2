namespace SIC.Domain.Entities;

public sealed class ClientCreditBalance
{
    public decimal VlrCreditos { get; set; }
    public decimal VlrTitulosEmAberto { get; set; }
    public decimal VlrPedidosNaoFaturados { get; set; }
}
