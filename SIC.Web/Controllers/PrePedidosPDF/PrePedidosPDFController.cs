using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.PrePedidosPDF;
using SIC.Web.Services.PrePedidosPDF;

namespace SIC.Web.Controllers.PrePedidosPDF;

[Authorize]
[Route("PrePedidosPDF")]
public sealed class PrePedidosPDFController(PrePedidoPDFApiClient apiClient) : Controller
{
    private static readonly IReadOnlyList<StatusPrePedidoPDFViewModel> StatusOptions =
    [
        new() { StatusPrePedidoPDFId = 1, Descricao = "Aguardando" },
        new() { StatusPrePedidoPDFId = 4, Descricao = "Aceito" },
        new() { StatusPrePedidoPDFId = 5, Descricao = "Recusado" },
    ];

    [HttpGet("List")]
    public async Task<IActionResult> List(
        int? status,
        string? cdExtCliente,
        string? dataInicial,
        string? dataFinal,
        CancellationToken cancellationToken)
    {
        var filtroStatus = status ?? 1;
        var filtroDataInicial = DateTime.TryParse(dataInicial, out var di) ? di : DateTime.Now.AddMonths(-1).Date;
        var filtroDataFinal = DateTime.TryParse(dataFinal, out var df) ? df : DateTime.Now.Date;
        var filtroAplicado = status.HasValue || !string.IsNullOrWhiteSpace(cdExtCliente)
            || !string.IsNullOrWhiteSpace(dataInicial) || !string.IsNullOrWhiteSpace(dataFinal);

        var statusFormatado = filtroStatus switch
        {
            1 => "Aguardando",
            4 => "Aceito",
            5 => "Recusado",
            6 => "Erro",
            0 => "Todos",
            _ => "Todos"
        };

        var dados = await apiClient.GetListAsync(filtroStatus, cdExtCliente, filtroDataInicial, filtroDataFinal, cancellationToken);

        var vm = new PrePedidoPDFListViewModel
        {
            FiltroStatus = filtroStatus,
            FiltroCdExtCliente = cdExtCliente,
            FiltroDataInicial = filtroDataInicial,
            FiltroDataFinal = filtroDataFinal,
            FiltroAplicado = filtroAplicado,
            StatusFormatado = statusFormatado,
            Dados = dados,
            StatusOptions = StatusOptions,
        };

        return View(vm);
    }
}
