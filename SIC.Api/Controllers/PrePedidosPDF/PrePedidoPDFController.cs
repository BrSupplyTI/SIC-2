using Microsoft.AspNetCore.Mvc;
using SIC.Api.Services.PrePedidosPDF;

namespace SIC.Api.Controllers.PrePedidosPDF;

/// <summary>
/// Endpoints de leitura (queries) do pré-pedido:
/// listagem, detalhes, itens, logs, endereços, locais de entrega, CNPJs, catálogo.
/// </summary>
[ApiController]
[Route("api/pre-pedidos-pdf")]
public sealed class PrePedidoPDFController(
    IPrePedidoPDFQueryService queryService,
    IPrePedidoPDFCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int? status,
        [FromQuery] string? cdExtCliente,
        [FromQuery] DateTime? dataInicial,
        [FromQuery] DateTime? dataFinal,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetListAsync(status, cdExtCliente, dataInicial, dataFinal, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("locais-entrega")]
    public async Task<IActionResult> GetLocaisEntrega(
        [FromQuery] int clienteEnderecoId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetLocaisEntregaAsync(clienteEnderecoId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("troca-itens")]
    public async Task<IActionResult> GetTrocaItens(
        [FromQuery] int tblPrecoId,
        [FromQuery] int estabelecimentoId,
        [FromQuery] int segmentoId,
        [FromQuery] int familiaId,
        [FromQuery] int itemId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetTrocaItensAsync(
            tblPrecoId,
            estabelecimentoId,
            segmentoId,
            familiaId,
            itemId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("buscar-catalogo")]
    public async Task<IActionResult> BuscarCatalogo(
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
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:int}/cnpj")]
    public async Task<IActionResult> AtualizarCnpj(
        int id,
        [FromBody] AtualizarCnpjRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarCnpjAsync(id, request.Cnpj, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}/endereco")]
    public async Task<IActionResult> AtualizarEndereco(
        int id,
        [FromBody] AtualizarEnderecoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarEnderecoAsync(
            id,
            request.ClienteEnderecoID,
            request.Logradouro,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}/local-entrega")]
    public async Task<IActionResult> AtualizarLocalEntrega(
        int id,
        [FromBody] AtualizarLocalEntregaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarLocalEntregaAsync(
            id,
            request.ClienteLocalEntregaID,
            request.NmLocalEntrega,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}/itens/{itemId:int}/quantidade")]
    public async Task<IActionResult> AtualizarQuantidade(
        int id,
        int itemId,
        [FromBody] AtualizarQuantidadeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarQuantidadeAsync(
            id,
            itemId,
            request.Quantidade,
            request.Descricao,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/itens/{itemId:int}/excluir")]
    public async Task<IActionResult> ExcluirItem(
        int id,
        int itemId,
        [FromBody] ExcluirItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.ExcluirItemAsync(id, itemId, request.Descricao, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/itens/{itemId:int}/trocar")]
    public async Task<IActionResult> TrocarItem(
        int id,
        int itemId,
        [FromBody] TrocarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.TrocarItemAsync(
            id,
            itemId,
            request.CdItem,
            request.ItemID,
            request.NmItem,
            request.VlrTabelaPreco,
            request.CdItemAntigo,
            request.DescricaoAntiga,
            request.ValorAntigo,
            request.MotivoTrocaItem,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/itens/adicionar")]
    public async Task<IActionResult> AdicionarItem(
        int id,
        [FromBody] AdicionarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AdicionarItemAsync(
            id,
            request.CodItemBR,
            request.DescrItemBR,
            request.Quantidade,
            request.PrecoTbl,
            request.ItemDePara,
            request.ItemID,
            request.OrdemCompra,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await commandService.CancelarAsync(id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/reprocessar")]
    public async Task<IActionResult> Reprocessar(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await commandService.ReprocessarAsync(id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/aceitar")]
    public async Task<IActionResult> AceitarPedido(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AceitarPedidoAsync(id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AtualizarCnpjRequest(string Cnpj);

    public sealed record AtualizarEnderecoRequest(
        int ClienteEnderecoID,
        string Logradouro);

    public sealed record AtualizarLocalEntregaRequest(
        int ClienteLocalEntregaID,
        string NmLocalEntrega);

    public sealed record AtualizarQuantidadeRequest(
        int Quantidade,
        string Descricao);

    public sealed record ExcluirItemRequest(string Descricao);

    public sealed record TrocarItemRequest(
        string CdItem,
        int ItemID,
        string NmItem,
        decimal VlrTabelaPreco,
        string CdItemAntigo,
        string DescricaoAntiga,
        string ValorAntigo,
        string MotivoTrocaItem);

    public sealed record AdicionarItemRequest(
        string CodItemBR,
        string DescrItemBR,
        int Quantidade,
        decimal PrecoTbl,
        string ItemDePara,
        int ItemID,
        string OrdemCompra);
}
