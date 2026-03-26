using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Produtos;
using SIC.Web.Services;

namespace SIC.Web.Controllers;

[Authorize]
[Route("Produtos")]
public sealed class ProdutosController(ProdutoApiClient apiClient) : Controller
{
    [HttpGet("Catalogo")]
    public async Task<IActionResult> Catalogo(
        string? texto,
        string tipoBusca = "comeca",
        int flagAtivo = 1,
        int flagMarcaPropria = 0,
        int? estabelecimentoId = null,
        int flagOutlet = 2,
        int flagSobDemanda = 2,
        int flagSustentavel = 0,
        int flagNovidade = 0,
        string? curva = null,
        int flagPadraoBrSupply = 1,
        int flagComEstoque = 0,
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

        var comecaComTexto = tipoBusca == "comeca" ? texto : null;
        var contemTexto = tipoBusca == "contem" ? texto : null;

        var estabelecimentos = await apiClient.GetEstablishmentsAsync(cancellationToken);

        var result = await apiClient.GetCatalogAsync(
            page, pageSize, comecaComTexto, contemTexto, flagAtivo, flagMarcaPropria,
            estabelecimentoId.Value, flagOutlet, flagSobDemanda,
            flagSustentavel, flagNovidade, curva, flagPadraoBrSupply, flagComEstoque,
            orderBy, cancellationToken);

        var vm = new ProdutoCatalogoViewModel
        {
            Texto = texto,
            TipoBusca = tipoBusca,
            FlagAtivo = flagAtivo,
            FlagMarcaPropria = flagMarcaPropria,
            EstabelecimentoID = estabelecimentoId.Value,
            FlagOutlet = flagOutlet,
            FlagSobDemanda = flagSobDemanda,
            FlagSustentavel = flagSustentavel,
            FlagNovidade = flagNovidade,
            Curva = curva,
            FlagPadraoBrSupply = flagPadraoBrSupply,
            FlagComEstoque = flagComEstoque,
            OrderBy = orderBy,
            PageNumber = result?.PageNumber ?? page,
            PageSize = result?.PageSize ?? pageSize,
            TotalRegistros = result?.TotalRegistros ?? 0,
            TotalPaginas = result?.TotalPaginas ?? 0,
            Itens = result?.Itens ?? [],
            Estabelecimentos = estabelecimentos
        };

        return View(vm);
    }
}
