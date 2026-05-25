using Microsoft.AspNetCore.Mvc;
using SIC.Api.Services.PrePedidosPDF;

namespace SIC.Api.Controllers.PrePedidosPDF;

/// <summary>
/// Endpoints de escrita (commands) do pré-pedido:
/// cancelar, reprocessar, gerar pedido, validar aceite,
/// atualizar endereço/local de entrega/CNPJ,
/// adicionar/excluir/atualizar itens, trocar item.
/// </summary>
[ApiController]
[Route("api/pre-pedidos-pdf")]
public sealed class PrePedidoPDFCommandController(IPrePedidoPDFCommandService service) : ControllerBase
{
    // POST api/pre-pedidos-pdf/{id}/cancelar
    // POST api/pre-pedidos-pdf/{id}/reprocessar
    // POST api/pre-pedidos-pdf/{id}/gerar-pedido
    // POST api/pre-pedidos-pdf/{id}/validar-aceite
    // PUT  api/pre-pedidos-pdf/{id}/endereco
    // PUT  api/pre-pedidos-pdf/{id}/local-entrega
    // PUT  api/pre-pedidos-pdf/{id}/cnpj
    // PUT  api/pre-pedidos-pdf/{id}/obs
    // POST api/pre-pedidos-pdf/{id}/itens
    // DELETE api/pre-pedidos-pdf/{id}/itens/{itemId}
    // PUT  api/pre-pedidos-pdf/{id}/itens/{itemId}/quantidade
    // PUT  api/pre-pedidos-pdf/{id}/itens/{itemId}/vlr-unit
    // PUT  api/pre-pedidos-pdf/{id}/itens/{itemId}/trocar

    [HttpPut("{id:int}/obs")]
    public async Task<IActionResult> AtualizarObs(
        int id,
        [FromBody] AtualizarObsRequest request,
        CancellationToken cancellationToken)
        => Ok(await service.AtualizarObsAsync(id, request.ObsNota ?? string.Empty, request.ObsComprador ?? string.Empty, cancellationToken));

    [HttpPut("{id:int}/itens/{itemId:int}/vlr-unit")]
    public async Task<IActionResult> AtualizarVlrUnit(
        int id,
        int itemId,
        [FromBody] AtualizarVlrUnitRequest request,
        CancellationToken cancellationToken)
        => Ok(await service.AtualizarVlrUnitAsync(id, itemId, request.VlrUnit, request.Descricao, cancellationToken));

    public sealed record AtualizarObsRequest(string? ObsNota, string? ObsComprador);
    public sealed record AtualizarVlrUnitRequest(decimal VlrUnit, string Descricao);
}
