using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Propostas;
using SIC.Api.Services.Propostas;

namespace SIC.Api.Controllers.Propostas;

[ApiController]
[Route("api/propostas")]
public sealed class PropostaController(IPropostaQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? filtroCodigo,
        [FromQuery] string? filtroNome,
        [FromQuery] string? filtroEstabelecimento,
        [FromQuery] string? filtroStatus,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetListAsync(filtroCodigo, filtroNome, filtroEstabelecimento, filtroStatus, cancellationToken);
        return Ok(result);
    }

    [HttpGet("segmentos")]
    public async Task<IActionResult> GetSegmentos(CancellationToken cancellationToken)
    {
        var result = await queryService.GetSegmentosAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(id, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:int}/codificacao")]
    public async Task<IActionResult> GetCodificacao(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.GetCodificacaoAsync(id, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Salvar(
        [FromBody] SalvarPropostaRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NomeProposta) || request.EstabelecimentoID <= 0)
            return BadRequest("Estabelecimento e Nome da Proposta são obrigatórios.");

        var result = await queryService.SalvarPropostaAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("buscar-itens")]
    public async Task<IActionResult> BuscarItens(
        [FromQuery] int estabelecimentoId,
        [FromQuery] string filtro,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filtro) || filtro.Length < 3)
            return BadRequest("Filtro deve ter pelo menos 3 caracteres.");

        var result = await queryService.BuscarItensBrSupplyAsync(estabelecimentoId, filtro, cancellationToken);
        return Ok(result);
    }

    [HttpPost("adicionar-item")]
    public async Task<IActionResult> AdicionarItem(
        [FromBody] AdicionarItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PropostaID <= 0 || request.ItemID <= 0 || request.QtdAnual <= 0)
            return BadRequest("PropostaID, ItemID e QtdAnual são obrigatórios.");

        var result = await queryService.AdicionarItemPropostaAsync(request, cancellationToken);
        return Ok(new { success = result });
    }

    [HttpDelete("{propostaId:int}/itens/{propostaItemId:int}")]
    public async Task<IActionResult> ExcluirItem(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.ExcluirItemPropostaAsync(propostaId, propostaItemId, cancellationToken);
        return Ok(new { success = result });
    }

    [HttpPost("importar-itens")]
    public async Task<IActionResult> ImportarItens(
        [FromBody] ImportarItensRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PropostaID <= 0 || request.Itens.Count == 0)
            return BadRequest("PropostaID e pelo menos um item são obrigatórios.");

        var inserted = await queryService.ImportarItensAsync(request, cancellationToken);
        return Ok(new { success = inserted > 0, inserted });
    }

    [HttpPost("{propostaItemId:int}/codificar-item")]
    public async Task<IActionResult> CodificarItem(
        int propostaItemId,
        [FromQuery] int estabelecimentoId,
        CancellationToken cancellationToken)
    {
        if (propostaItemId <= 0 || estabelecimentoId <= 0)
            return BadRequest("PropostaItemID e EstabelecimentoID são obrigatórios.");

        var result = await queryService.CodificarItemAsync(propostaItemId, estabelecimentoId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{propostaId:int}/codificar-segundo-plano")]
    public async Task<IActionResult> CodificarSegundoPlano(
        int propostaId,
        CancellationToken cancellationToken)
    {
        if (propostaId <= 0)
            return BadRequest("PropostaID é obrigatório.");

        var result = await queryService.MarcarSegundoPlanoAsync(propostaId, cancellationToken);
        return Ok(new { success = result });
    }

    [HttpDelete("{propostaId:int}")]
    public async Task<IActionResult> ExcluirProposta(
        int propostaId,
        CancellationToken cancellationToken)
    {
        if (propostaId <= 0)
            return BadRequest("PropostaID é obrigatório.");

        var result = await queryService.ExcluirPropostaAsync(propostaId, cancellationToken);
        return Ok(new { success = result });
    }

    [HttpPost("{propostaItemId:int}/vincular-item-manual")]
    public async Task<IActionResult> VincularItemManual(
        int propostaItemId,
        [FromQuery] int itemId,
        CancellationToken cancellationToken)
    {
        if (propostaItemId <= 0 || itemId <= 0)
            return BadRequest("PropostaItemID e ItemID são obrigatórios.");

        var result = await queryService.VincularItemManualAsync(propostaItemId, itemId, cancellationToken);
        return Ok(new { success = result });
    }
}
