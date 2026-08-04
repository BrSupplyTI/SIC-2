namespace SIC.Api.Contracts.IntegracaoClientes.ProcessadoresPedidos;

public sealed class ProcessadorPedidoConfiguracaoDto
{
    public int ProcessadorPedidoId { get; set; }
    public int ClienteId { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string RazaoSocialCliente { get; set; } = string.Empty;
    public string DeParaCliente { get; set; } = string.Empty;
}
