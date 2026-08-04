using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.IntegracaoClientes.ProcessadoresPedidos;
using SIC.Web.Services.IntegracaoClientes;

namespace SIC.Web.Controllers.IntegracaoClientes.ProcessadoresPedidos;

[Authorize]
[Route("IntegracaoClientes/ProcessadoresPedidos")]
public sealed class ProcessadoresPedidosController(ProcessadoresPedidosApiClient apiClient) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int? processadorPedidoId, CancellationToken cancellationToken)
    {
        var processadores = await apiClient.GetAllAsync(cancellationToken);

        var configuracoes = processadorPedidoId.HasValue && processadorPedidoId.Value > 0
            ? await apiClient.GetConfiguracoesAsync(processadorPedidoId.Value, cancellationToken)
            : [];

        var vm = new ProcessadoresPedidosIndexViewModel
        {
            ProcessadorPedidoIdSelecionado = processadorPedidoId,
            ProcessadoresPedido = processadores,
            Configuracoes = configuracoes
        };

        return View("~/Views/IntegracaoClientes/ProcessadoresPedidos/Index.cshtml", vm);
    }
}
