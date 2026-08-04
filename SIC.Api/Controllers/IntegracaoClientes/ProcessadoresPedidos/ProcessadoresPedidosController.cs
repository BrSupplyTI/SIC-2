using Microsoft.AspNetCore.Mvc;
using SIC.Api.Services.IntegracaoClientes.ProcessadoresPedidos;

namespace SIC.Api.Controllers.IntegracaoClientes.ProcessadoresPedidos;

[ApiController]
[Route("api/integracao-clientes/processadores-pedidos")]
public sealed class ProcessadoresPedidosController(IProcessadorPedidoService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
		try
		{
            var items = await service.GetAllAsync(cancellationToken);
            return Ok(items);
        }
		catch (Exception e)
		{
            return BadRequest(e.Message);
		}
        
    }

    [HttpGet("{processadorPedidoId:int}/configuracoes")]
    public async Task<IActionResult> GetConfiguracoes(int processadorPedidoId, CancellationToken cancellationToken)
    {
        try
        {
            var items = await service.GetConfiguracoesAsync(processadorPedidoId, cancellationToken);
            return Ok(items);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
