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
    // POST api/pre-pedidos-pdf/{id}/itens
    // DELETE api/pre-pedidos-pdf/{id}/itens/{itemId}
    // PUT  api/pre-pedidos-pdf/{id}/itens/{itemId}/quantidade
    // PUT  api/pre-pedidos-pdf/{id}/itens/{itemId}/trocar
}
