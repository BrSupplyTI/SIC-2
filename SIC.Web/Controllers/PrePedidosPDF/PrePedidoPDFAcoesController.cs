using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Services.PrePedidosPDF;

namespace SIC.Web.Controllers.PrePedidosPDF;

/// <summary>
/// Ações de workflow do pré-pedido: cancelar, reprocessar, gerar pedido, validar aceite.
/// Views resolvidas explicitamente em ~/Views/PrePedidosPDF/.
/// </summary>
[Authorize]
[Route("PrePedidosPDF")]
public sealed class PrePedidoPDFAcoesController(PrePedidoPDFApiClient apiClient) : Controller
{
    // Futuro: CancelarPrePedidoPDF, ReprocessarPedido, ValidarParaAceite, GerarPedido
}
