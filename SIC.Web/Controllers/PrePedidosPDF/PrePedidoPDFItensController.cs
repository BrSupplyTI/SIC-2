using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Services.PrePedidosPDF;

namespace SIC.Web.Controllers.PrePedidosPDF;

/// <summary>
/// Gestão de itens do pré-pedido: adicionar, excluir, atualizar quantidade, trocar.
/// Views resolvidas explicitamente em ~/Views/PrePedidosPDF/.
/// </summary>
[Authorize]
[Route("PrePedidosPDF")]
public sealed class PrePedidoPDFItensController(PrePedidoPDFApiClient apiClient) : Controller
{
    // Futuro: AdicionarItens, ExcluirItem, UpdateQuantidade, TrocarItem, BuscarCatalogo
}
