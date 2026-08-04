using SIC.Api.Contracts.IntegracaoClientes.ProcessadoresPedidos;

namespace SIC.Api.Services.IntegracaoClientes.ProcessadoresPedidos;

public interface IProcessadorPedidoService
{
    Task<IReadOnlyList<ProcessadorPedidoDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProcessadorPedidoConfiguracaoDto>> GetConfiguracoesAsync(int processadorPedidoId, CancellationToken cancellationToken = default);
}
