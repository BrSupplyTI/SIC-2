namespace SIC.Domain.Entities.IntegracaoClientes.ProcessadoresPedidos
{
    public sealed class ProcessadorPedidoConfiguracao
    {
        public int ProcessadorPedidoId { get; set; }
        public int ClienteId { get; set; }
        public string CodigoCliente { get; set; } = string.Empty;
        public string RazaoSocialCliente { get; set; } = string.Empty;
        public string DeParaCliente { get; set; } = string.Empty;
    }
}
