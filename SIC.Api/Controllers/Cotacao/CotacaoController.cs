using Microsoft.AspNetCore.Mvc;
using SIC.Api.Services.Cotacao;

namespace SIC.Api.Controllers.Cotacao;

/// <summary>
/// Endpoints de consulta e escrita da Cotação (Proposta).
/// </summary>
[ApiController]
[Route("api/cotacao")]
public sealed class CotacaoController(
    ICotacaoQueryService queryService,
    ICotacaoCommandService commandService) : ControllerBase
{
    [HttpGet("{propostaId:int}/buscar-catalogo")]
    public async Task<IActionResult> BuscarCatalogo(
        int propostaId,
        [FromQuery] string descricao,
        [FromQuery] int clienteId,
        [FromQuery] int tblPrecoId,
        [FromQuery] int estabelecimentoId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.BuscarCatalogoAsync(
            descricao,
            clienteId,
            tblPrecoId,
            estabelecimentoId,
            propostaId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{propostaId:int}/itens/adicionar")]
    public async Task<IActionResult> AdicionarItem(
        int propostaId,
        [FromBody] AdicionarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AdicionarItemAsync(
            propostaId,
            request.CodItemBR,
            request.DescrItemBR,
            request.TipoCusto,
            request.PrecoItem,
            request.VlrCustoAquisicao,
            request.VlrCustoMedio,
            request.Quantidade,
            request.VlrPrecoMinimo,
            request.VlrTabelaPreco,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AdicionarItemRequest(
        string CodItemBR,
        string DescrItemBR,
        string TipoCusto,
        decimal PrecoItem,
        decimal VlrCustoAquisicao,
        decimal VlrCustoMedio,
        int Quantidade,
        decimal VlrPrecoMinimo,
        decimal VlrTabelaPreco);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/calcular-margem")]
    public async Task<IActionResult> CalcularMargemItem(
        int propostaId,
        int propostaItemId,
        [FromBody] CalcularMargemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.CalcularMargemItemAsync(
            propostaId,
            propostaItemId,
            request.Type,
            request.ViaTela,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record CalcularMargemRequest(string Type, string ViaTela);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/atualizar")]
    public async Task<IActionResult> AtualizarItem(
        int propostaId,
        int propostaItemId,
        [FromBody] AtualizarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarItemAsync(
            propostaId,
            propostaItemId,
            request.PrecoUnitario,
            request.Quantidade,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AtualizarItemRequest(decimal PrecoUnitario, decimal Quantidade);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/atualizar-custo")]
    public async Task<IActionResult> AtualizarCustoItem(
        int propostaId,
        int propostaItemId,
        [FromBody] AtualizarCustoItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarCustoItemAsync(
            propostaId,
            propostaItemId,
            request.TipoCusto,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AtualizarCustoItemRequest(string TipoCusto);

    [HttpPost("{propostaId:int}/gerar-itens")]
    public async Task<IActionResult> GerarItens(
        int propostaId,
        [FromBody] GerarItensRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.GerarItensAsync(
            propostaId,
            request.TipoGeracao,
            request.UsuarioID,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record GerarItensRequest(string TipoGeracao, int UsuarioID);

    public sealed record RemoverItensRequest(List<RemoverItemInfo> Itens, string Motivo, int UsuarioId);
    public sealed record RemoverItemInfo(int PropostaItemId, string CdItem);

    [HttpPost("{propostaId:int}/itens/remover")]
    public async Task<IActionResult> RemoverItens(
        int propostaId,
        [FromBody] RemoverItensRequest request,
        CancellationToken cancellationToken)
    {
        var itens = request.Itens
            .Select(i => (i.PropostaItemId, i.CdItem))
            .ToList();

        var result = await commandService.RemoverItensAsync(
            propostaId, itens, request.Motivo, request.UsuarioId, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record SalvarCondPagtoRequest(int CondPagtoId);

    [HttpPost("{propostaId:int}/salvar-cond-pagto")]
    public async Task<IActionResult> SalvarCondPagto(
        int propostaId,
        [FromBody] SalvarCondPagtoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.SalvarCondPagtoAsync(
            propostaId, request.CondPagtoId, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{propostaId:int}/recalcular-margem-bruta")]
    public async Task<IActionResult> RecalcularMargemBruta(
        int propostaId,
        CancellationToken cancellationToken)
    {
        var result = await commandService.RecalcularMargemBrutaPropostaAsync(
            propostaId, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
