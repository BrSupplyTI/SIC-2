namespace SIC.Web.Models.IntegracaoClientes.ProcessadoresPedidos;

public sealed class ProcessadorPedidoConfiguracaoVm
{
    public int ProcessadorPedidoId { get; set; }
    public int ClienteId { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string RazaoSocialCliente { get; set; } = string.Empty;
    public string DeParaCliente { get; set; } = string.Empty;
}
