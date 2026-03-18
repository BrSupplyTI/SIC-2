using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Pedidos;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class OrderSearchController(IOrderSearchService service) : ControllerBase
{
    [HttpGet("{pedido:int}/detalhes-cabecalho")]
    public async Task<IActionResult> GetOrderHeaderDetails(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderHeaderDetailsAsync(pedido, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("buscar-por-pedido")]
    public async Task<IActionResult> SearchByOrderNumber([FromBody] SearchByOrderNumberRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SearchByOrderNumberAsync(request.NumeroPedido, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("buscar-por-ordem-compra")]
    public async Task<IActionResult> SearchByPurchaseOrder([FromBody] SearchByPurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SearchByPurchaseOrderAsync(request.OrdemCompra, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("buscar-por-nota-fiscal")]
    public async Task<IActionResult> SearchByInvoice([FromBody] SearchByInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await service.SearchByInvoiceAsync(request.NotaFiscal, request.Serie, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
