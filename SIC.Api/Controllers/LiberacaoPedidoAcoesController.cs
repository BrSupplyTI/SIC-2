using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Liberacao;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/liberacao-pedidos/acoes")]
public sealed class LiberacaoPedidoAcoesController(ILiberacaoPedidoAcoesService service) : ControllerBase
{
    // ---------- Queries auxiliares ----------

    [HttpGet("canais-venda")]
    public async Task<IActionResult> ListarCanaisVenda([FromQuery] int usuarioId, [FromQuery] string nmCanalAtual, CancellationToken ct)
    {
        if (usuarioId <= 0) return BadRequest("UsuarioId é obrigatório.");
        var result = await service.ListarCanaisVendaAsync(usuarioId, nmCanalAtual ?? string.Empty, ct);
        return Ok(result);
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> ListarCategorias([FromQuery] int clienteId, CancellationToken ct)
    {
        if (clienteId <= 0) return BadRequest("ClienteId é obrigatório.");
        var result = await service.ListarCategoriasAsync(clienteId, ct);
        return Ok(result);
    }

    [HttpGet("condicoes-pagamento")]
    public async Task<IActionResult> ListarCondicoesPagamento([FromQuery] string nmCondPagtoAtual, CancellationToken ct)
    {
        var result = await service.ListarCondicoesPagamentoAsync(nmCondPagtoAtual ?? string.Empty, ct);
        return Ok(result);
    }

    [HttpGet("{cotacaoId:int}/opcoes-frete")]
    public async Task<IActionResult> ListarOpcoesFrete(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest("CotacaoId é obrigatório.");
        var result = await service.ListarOpcoesFreteAsync(cotacaoId, ct);
        return Ok(result);
    }

    [HttpGet("{cotacaoId:int}/impostos")]
    public async Task<IActionResult> ListarImpostos(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest("CotacaoId é obrigatório.");
        var result = await service.ListarImpostosAsync(cotacaoId, ct);
        return Ok(result);
    }

    // ---------- Logs (Fase 4) ----------

    [HttpGet("{cotacaoId:int}/cotlog")]
    public async Task<IActionResult> ListarCotLog(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Ok(await service.ListarCotLogAsync(cotacaoId, ct));
    }

    [HttpGet("{cotacaoId:int}/backofficelog")]
    public async Task<IActionResult> ListarBackOfficeLog(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Ok(await service.ListarBackOfficeLogAsync(cotacaoId, ct));
    }

    [HttpGet("{cotacaoId:int}/cotlog-detalhado")]
    public async Task<IActionResult> ListarCotLogDetalhado(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Ok(await service.ListarCotLogDetalhadoAsync(cotacaoId, ct));
    }

    // ---------- Itens (Fase 5) ----------

    [HttpGet("{cotacaoId:int}/itens-brsupply")]
    public async Task<IActionResult> ListarItensBrSupply(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Ok(await service.ListarItensBrSupplyAsync(cotacaoId, ct));
    }

    [HttpGet("{cotacaoId:int}/itens-marketplace")]
    public async Task<IActionResult> ListarItensMarketplace(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Ok(await service.ListarItensMarketplaceAsync(cotacaoId, ct));
    }

    [HttpGet("item/{cotacaoItemId:int}/compativeis")]
    public async Task<IActionResult> ListarItensCompativeis(int cotacaoItemId, CancellationToken ct)
    {
        if (cotacaoItemId <= 0) return BadRequest();
        return Ok(await service.BuscarCompativeisTrocaAsync(cotacaoItemId, ct));
    }

    [HttpPost("item/alterar")]
    public async Task<IActionResult> AlterarItem([FromBody] AlterarItemRequest req, CancellationToken ct)
        => Ok(await service.AlterarItemAsync(req, ct));

    [HttpPost("item/alterar-com-ov")]
    public async Task<IActionResult> AlterarItemComOv([FromBody] AlterarItemComOvRequest req, CancellationToken ct)
        => Ok(await service.AlterarItemComOvAsync(req, ct));

    [HttpPost("item/excluir")]
    public async Task<IActionResult> ExcluirItem([FromBody] ExcluirItemRequest req, CancellationToken ct)
        => Ok(await service.ExcluirItemAsync(req, ct));

    [HttpPost("item/trocar")]
    public async Task<IActionResult> TrocarItem([FromBody] TrocarItemRequest req, CancellationToken ct)
        => Ok(await service.TrocarItemAsync(req, ct));

    // ---------- Ações de escrita ----------

    [HttpPost("alterar-obs-nota")]
    public async Task<IActionResult> AlterarObsNota([FromBody] AlterarObservacaoRequest req, CancellationToken ct)
        => Ok(await service.AlterarObsNotaAsync(req, ct));

    [HttpPost("alterar-obs-solicitante")]
    public async Task<IActionResult> AlterarObsSolicitante([FromBody] AlterarObservacaoRequest req, CancellationToken ct)
        => Ok(await service.AlterarObsSolicitanteAsync(req, ct));

    [HttpPost("alterar-obs-aprovador")]
    public async Task<IActionResult> AlterarObsAprovador([FromBody] AlterarObservacaoRequest req, CancellationToken ct)
        => Ok(await service.AlterarObsAprovadorAsync(req, ct));

    [HttpPost("alterar-ordem-compra")]
    public async Task<IActionResult> AlterarOrdemCompra([FromBody] AlterarOrdemCompraRequest req, CancellationToken ct)
        => Ok(await service.AlterarOrdemCompraAsync(req, ct));

    [HttpPost("alterar-canal-venda")]
    public async Task<IActionResult> AlterarCanalVenda([FromBody] AlterarCanalVendaRequest req, CancellationToken ct)
        => Ok(await service.AlterarCanalVendaAsync(req, ct));

    [HttpPost("alterar-categoria")]
    public async Task<IActionResult> AlterarCategoria([FromBody] AlterarCategoriaRequest req, CancellationToken ct)
        => Ok(await service.AlterarCategoriaAsync(req, ct));

    [HttpPost("alterar-cond-pagto")]
    public async Task<IActionResult> AlterarCondPagto([FromBody] AlterarCondPagtoRequest req, CancellationToken ct)
        => Ok(await service.AlterarCondPagtoAsync(req, ct));

    [HttpPost("cobrar-frete")]
    public async Task<IActionResult> CobrarFrete([FromBody] CobrarFreteRequest req, CancellationToken ct)
        => Ok(await service.CobrarFreteAsync(req, ct));

    [HttpPost("liberar-marketplace")]
    public async Task<IActionResult> LiberarMarketplace([FromBody] LiberarMarketplaceRequest req, CancellationToken ct)
        => Ok(await service.LiberarMarketplaceAsync(req, ct));

    [HttpPost("cancelar-pedido")]
    public async Task<IActionResult> CancelarPedido([FromBody] CancelarPedidoRequest req, CancellationToken ct)
        => Ok(await service.CancelarPedidoAsync(req, ct));

    [HttpPost("cancelar-marketplace")]
    public async Task<IActionResult> CancelarMarketplace([FromBody] CancelarPedidoRequest req, CancellationToken ct)
        => Ok(await service.CancelarMarketplaceAsync(req, ct));

    [HttpPost("desbloquear-alocacoes")]
    public async Task<IActionResult> DesbloquearAlocacoes([FromBody] DesbloquearAlocacoesRequest req, CancellationToken ct)
        => Ok(await service.DesbloquearAlocacoesAsync(req, ct));

    [HttpPost("gerar-pedido-rupturas")]
    public async Task<IActionResult> GerarPedidoRupturas([FromBody] GerarPedidoRupturasRequest req, CancellationToken ct)
        => Ok(await service.GerarPedidoRupturasAsync(req, ct));
}
