using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Categorizacao;
using SIC.Domain.Abstractions.Categorizacao;

namespace SIC.Api.Controllers.Categorizacao;

[ApiController]
[Route("api/categorizacao")]
public sealed class CategorizacaoController(ICategorizacaoRepository repository) : ControllerBase
{
    [HttpGet("itens")]
    public async Task<IActionResult> ItensCategorizados([FromQuery] int? estabelecimentoId, CancellationToken ct)
    {
        var itens = await repository.GetItensCategorizadosAsync(estabelecimentoId, ct);
        var dtos  = itens.Select(i => new CategorizacaoItemDto(
            i.ItemID, i.CdItem, i.NmItem, i.NmEstabelecimento,
            i.Criticidade,
            i.VlrCustoAquisicao.HasValue
                ? i.VlrCustoAquisicao.Value.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))
                : "—",
            i.QtDispEstoque,
            i.NmTipoLista, i.PesquisaTipoListaID, i.Prioridade));
        return Ok(dtos);
    }

    [HttpGet("itens-sem-categoria")]
    public async Task<IActionResult> ItensSemCategoria(CancellationToken ct)
    {
        var itens = await repository.GetItensSemCategoriaAsync(ct);
        var dtos  = itens.Select(i => new CategorizacaoItemSemCategoriaDto(
            i.ItemID, i.CdItem, i.NmItem, i.NmSegmento));
        return Ok(dtos);
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> Categorias(CancellationToken ct)
    {
        var cats = await repository.GetCategoriasAsync(ct);
        var dtos = cats.Select(c => new CategorizacaoTipoListaDto(c.PesquisaTipoListaID, c.NmTipoLista));
        return Ok(dtos);
    }

    [HttpPost("salvar-categoria")]
    public async Task<IActionResult> SalvarCategoria([FromBody] SalvarCategoriaRequest request, CancellationToken ct)
    {
        var ok = await repository.SalvarCategoriaAsync(request.ItemID, request.PesquisaTipoListaID, ct);
        return Ok(new { success = ok });
    }

    [HttpDelete("remover-categoria/{itemId:int}")]
    public async Task<IActionResult> RemoverCategoria(int itemId, CancellationToken ct)
    {
        var ok = await repository.RemoverCategoriaAsync(itemId, ct);
        return Ok(new { success = ok });
    }
}
