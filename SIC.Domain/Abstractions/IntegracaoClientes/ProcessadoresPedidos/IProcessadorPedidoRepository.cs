using SIC.Domain.Entities.IntegracaoClientes.ProcessadoresPedidos;

namespace SIC.Domain.Abstractions.IntegracaoClientes.ProcessadoresPedidos
{
    public interface IProcessadorPedidoRepository
    {
        Task<List<ProcessadorPedido>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<ProcessadorPedidoConfiguracao>> GetConfiguracoesAsync(int processadorPedidoId, CancellationToken cancellationToken = default);
    }
}
