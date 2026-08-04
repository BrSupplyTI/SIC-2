using SIC.Api.Contracts.IntegracaoClientes.ProcessadoresPedidos;
using SIC.Domain.Abstractions.IntegracaoClientes.ProcessadoresPedidos;

namespace SIC.Api.Services.IntegracaoClientes.ProcessadoresPedidos;

public sealed class ProcessadorPedidoService(IProcessadorPedidoRepository repository) : IProcessadorPedidoService
{
    public async Task<IReadOnlyList<ProcessadorPedidoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetAllAsync(cancellationToken);

        return items.Select(item => new ProcessadorPedidoDto
        {
            ProcessadorPedidoId = item.ProcessadorPedidoId,
            Nome = item.Nome ?? string.Empty
        }).ToList();
    }

    public async Task<IReadOnlyList<ProcessadorPedidoConfiguracaoDto>> GetConfiguracoesAsync(int processadorPedidoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetConfiguracoesAsync(processadorPedidoId, cancellationToken);

        return items.Select(item => new ProcessadorPedidoConfiguracaoDto
        {
            ProcessadorPedidoId = item.ProcessadorPedidoId,
            ClienteId = item.ClienteId,
            CodigoCliente = item.CodigoCliente,
            RazaoSocialCliente = item.RazaoSocialCliente,
            DeParaCliente = item.DeParaCliente
        }).ToList();
    }
}
