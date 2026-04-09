namespace SIC.Web.Models.Clientes;

public sealed class ClientCreditBalanceVm
{
    public decimal VlrCreditos { get; set; }
    public decimal VlrTitulosEmAberto { get; set; }
    public decimal VlrPedidosNaoFaturados { get; set; }
}
