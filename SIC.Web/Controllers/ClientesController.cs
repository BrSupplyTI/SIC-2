using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Clientes;
using SIC.Web.Services;

namespace SIC.Web.Controllers;

[Authorize]
[Route("Clientes")]
public sealed class ClientesController(ClienteApiClient apiClient) : Controller
{
    [HttpGet("Busca")]
    public async Task<IActionResult> Busca(
        string? texto,
        string tipoBusca = "comeca",
        int flagAtivo = 1,
        int? estabelecimentoId = null,
        int flagClienteMae = 0,
        int carteiraId = 0,
        int qtDiasUltimoPedido = 0,
        string orderBy = "Nome (A-Z)",
        int page = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        if (estabelecimentoId is null)
        {
            var claim = User.FindFirst("sic_estabelecimentoid")?.Value;
            estabelecimentoId = int.TryParse(claim, out var id) ? id : 0;
        }

        var usuarioIdClaim = User.FindFirst("sic_usuarioid")?.Value;
        var usuarioId = int.TryParse(usuarioIdClaim, out var uid) ? uid : 0;

        var comecaComTexto = tipoBusca == "comeca" ? texto : null;
        var contemTexto = tipoBusca == "contem" ? texto : null;

        var estabelecimentos = await apiClient.GetEstablishmentsAsync(cancellationToken);
        var carteiras = await apiClient.GetWalletsAsync(cancellationToken);

        var result = await apiClient.SearchAsync(
            page, pageSize, comecaComTexto, contemTexto, flagAtivo,
            estabelecimentoId.Value, flagClienteMae, carteiraId,
            qtDiasUltimoPedido, orderBy, usuarioId, cancellationToken);

        var vm = new ClienteBuscaViewModel
        {
            Texto = texto,
            TipoBusca = tipoBusca,
            FlagAtivo = flagAtivo,
            EstabelecimentoID = estabelecimentoId.Value,
            FlagClienteMae = flagClienteMae,
            CarteiraID = carteiraId,
            QtDiasUltimoPedido = qtDiasUltimoPedido,
            OrderBy = orderBy,
            PageNumber = result?.PageNumber ?? page,
            PageSize = result?.PageSize ?? pageSize,
            TotalRegistros = result?.TotalRegistros ?? 0,
            TotalPaginas = result?.TotalPaginas ?? 0,
            Itens = result?.Itens ?? [],
            Estabelecimentos = estabelecimentos,
            Carteiras = carteiras
        };

        return View(vm);
    }

    [HttpGet("Detalhes/{clienteId:int}")]
    public async Task<IActionResult> Detalhes(int clienteId, CancellationToken cancellationToken)
    {
        var vm = await apiClient.GetClientDetailAsync(clienteId, cancellationToken);
        if (vm is null) return NotFound();

        vm.Consultores = await apiClient.GetConsultantsAsync(clienteId, cancellationToken);

        return View(vm);
    }

    [HttpGet("Detalhes/{clienteId:int}/Titulos")]
    public async Task<IActionResult> Titulos(int clienteId, CancellationToken cancellationToken)
    {
        var data = await apiClient.GetTitulosAsync(clienteId, cancellationToken);
        return Json(data);
    }
}
