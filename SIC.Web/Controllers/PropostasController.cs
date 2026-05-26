using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Propostas;
using SIC.Web.Services;
using SIC.Web.Services.Propostas;

namespace SIC.Web.Controllers;

[Authorize]
[Route("Propostas")]
public sealed class PropostasController(ProdutoApiClient produtoApiClient, PropostaApiClient propostaApiClient) : Controller
{
    [HttpGet("")]
    [HttpGet("Lista")]
    public async Task<IActionResult> Lista(string? filtroCodigo, string? filtroNome, string? filtroEstabelecimento, string? filtroStatus, CancellationToken cancellationToken)
    {
        var propostas = await propostaApiClient.GetListAsync(filtroCodigo, filtroNome, filtroEstabelecimento, filtroStatus, cancellationToken);

        var vm = new PropostasListViewModel
        {
            FiltroCodigo = filtroCodigo,
            FiltroNome = filtroNome,
            FiltroEstabelecimento = filtroEstabelecimento,
            FiltroStatus = filtroStatus,
            Propostas = propostas
        };

        return View(vm);
    }

    [HttpGet("CadastroProposta")]
    public async Task<IActionResult> CadastroProposta(int? id, CancellationToken cancellationToken)
    {
        var estabelecimentos = await produtoApiClient.GetEstablishmentsAsync(cancellationToken);
        var segmentos = await propostaApiClient.GetSegmentosAsync(cancellationToken);

        var vm = new CadastroPropostaViewModel
        {
            Estabelecimentos = estabelecimentos,
            Segmentos = segmentos
        };

        if (id.HasValue)
        {
            var proposta = await propostaApiClient.GetByIdAsync(id.Value, cancellationToken);
            if (proposta is not null)
            {
                vm.PropostaID = proposta.PropostaID;
                vm.EstabelecimentoID = proposta.EstabelecimentoID;
                vm.NomeProposta = proposta.NomeProposta;
                vm.QualSegCadastrados = proposta.QualSeg;
            }
        }

        return View(vm);
    }

    [HttpPost("CadastroProposta")]
    public async Task<IActionResult> CadastroPropostaSalvar(SalvarPropostaRequestVm request, CancellationToken cancellationToken)
    {
        var result = await propostaApiClient.SalvarPropostaAsync(request, cancellationToken);

        if (result is not null && result.PropostaID > 0)
        {
            TempData["SuccessMessage"] = "Proposta salva com sucesso!";
            return RedirectToAction("Codificacao", new { id = result.PropostaID });
        }

        TempData["ErrorMessage"] = "Ocorreu um erro ao salvar a proposta.";
        return RedirectToAction("CadastroProposta");
    }

    [HttpGet("Codificacao/{id:int}")]
    public async Task<IActionResult> Codificacao(int id, CancellationToken cancellationToken)
    {
        var vm = await propostaApiClient.GetCodificacaoAsync(id, cancellationToken);
        if (vm is null)
            return RedirectToAction("Lista");

        return View(vm);
    }

    [HttpGet("BuscarItens")]
    public async Task<IActionResult> BuscarItens(int estabelecimentoId, string filtro, CancellationToken cancellationToken)
    {
        var result = await propostaApiClient.BuscarItensBrSupplyAsync(estabelecimentoId, filtro, cancellationToken);
        return Json(result);
    }

    [HttpPost("AdicionarItem")]
    public async Task<IActionResult> AdicionarItem([FromBody] AdicionarItemRequestVm request, CancellationToken cancellationToken)
    {
        var success = await propostaApiClient.AdicionarItemPropostaAsync(request, cancellationToken);
        return Json(new { success });
    }

    [HttpDelete("ExcluirItem/{propostaId:int}/{propostaItemId:int}")]
    public async Task<IActionResult> ExcluirItem(int propostaId, int propostaItemId, CancellationToken cancellationToken)
    {
        var success = await propostaApiClient.ExcluirItemPropostaAsync(propostaId, propostaItemId, cancellationToken);
        return Json(new { success });
    }

    [HttpPost("ImportarItens")]
    public async Task<IActionResult> ImportarItens([FromBody] ImportarItensRequestVm request, CancellationToken cancellationToken)
    {
        var (success, inserted) = await propostaApiClient.ImportarItensAsync(request, cancellationToken);
        return Json(new { success, inserted });
    }

    [HttpPost("CodificarItem")]
    public async Task<IActionResult> CodificarItem(int propostaItemId, int estabelecimentoId, CancellationToken cancellationToken)
    {
        var result = await propostaApiClient.CodificarItemAsync(propostaItemId, estabelecimentoId, cancellationToken);
        if (result is null)
            return Json(new { codificado = false, semCorrespondencia = true });
        return Json(result);
    }

    [HttpPost("CodificarSegundoPlano")]
    public async Task<IActionResult> CodificarSegundoPlano(int propostaId, CancellationToken cancellationToken)
    {
        var success = await propostaApiClient.MarcarSegundoPlanoAsync(propostaId, cancellationToken);
        return Json(new { success });
    }

    [HttpPost("Excluir/{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var success = await propostaApiClient.ExcluirPropostaAsync(id, cancellationToken);

        TempData["SuccessMessage"] = success ? "Proposta excluída com sucesso." : null;
        TempData["ErrorMessage"] = success ? null : "Erro ao excluir a proposta.";

        return RedirectToAction(nameof(Lista));
    }

    [HttpPost("VincularItemManual")]
    public async Task<IActionResult> VincularItemManual(int propostaItemId, int itemId, CancellationToken cancellationToken)
    {
        var success = await propostaApiClient.VincularItemManualAsync(propostaItemId, itemId, cancellationToken);
        return Json(new { success });
    }
}
