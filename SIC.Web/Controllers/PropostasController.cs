using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Propostas;
using SIC.Web.Services;

namespace SIC.Web.Controllers;

[Authorize]
[Route("Propostas")]
public sealed class PropostasController(ProdutoApiClient produtoApiClient) : Controller
{
    [HttpGet("CadastroProposta")]
    public async Task<IActionResult> CadastroProposta(CancellationToken cancellationToken)
    {
        var estabelecimentos = await produtoApiClient.GetEstablishmentsAsync(cancellationToken);

        var vm = new CadastroPropostaViewModel
        {
            Estabelecimentos = estabelecimentos
        };

        return View(vm);
    }
}
