using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Services.PrePedidosPDF;

namespace SIC.Web.Controllers.PrePedidosPDF;

/// <summary>
/// Consulta de detalhes do pré-pedido: detalhe, itens, endereços, logs.
/// Views resolvidas explicitamente em ~/Views/PrePedidosPDF/.
/// </summary>
[Authorize]
[Route("PrePedidosPDF")]
public sealed class PrePedidoPDFDetalhesController(PrePedidoPDFApiClient apiClient) : Controller
{
    [HttpGet("PrePedido/{id:int}")]
    public async Task<IActionResult> PrePedido(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return RedirectToAction("List", "PrePedidosPDF");

        var model = await apiClient.GetByIdAsync(id, cancellationToken);

        if (model is null)
            return RedirectToAction("List", "PrePedidosPDF");

        return View("~/Views/PrePedidosPDF/PrePedido.cshtml", model);
    }

    [HttpGet("PrePedido/{id:int}/itens")]
    public async Task<IActionResult> GetItens(int id, CancellationToken cancellationToken)
        => Json(await apiClient.GetItensAsync(id, cancellationToken));

    [HttpGet("PrePedido/{id:int}/logs")]
    public async Task<IActionResult> GetLogs(int id, CancellationToken cancellationToken)
        => Json(await apiClient.GetLogsAsync(id, cancellationToken));

    [HttpGet("PrePedido/locais-entrega")]
    public async Task<IActionResult> GetLocaisEntrega(
        [FromQuery] int clienteEnderecoId,
        CancellationToken cancellationToken)
        => Json(await apiClient.GetLocaisEntregaAsync(clienteEnderecoId, cancellationToken));

    [HttpGet("PrePedido/troca-itens")]
    public async Task<IActionResult> GetTrocaItens(
        [FromQuery] int tblPrecoId,
        [FromQuery] int estabelecimentoId,
        [FromQuery] int segmentoId,
        [FromQuery] int familiaId,
        [FromQuery] int itemId,
        CancellationToken cancellationToken)
        => Json(await apiClient.GetTrocaItensAsync(
            tblPrecoId,
            estabelecimentoId,
            segmentoId,
            familiaId,
            itemId,
            cancellationToken));

    [HttpGet("PrePedido/buscar-catalogo")]
    public async Task<IActionResult> BuscarCatalogo(
        [FromQuery] string descricao,
        [FromQuery] int clienteId,
        [FromQuery] int tblPrecoId,
        [FromQuery] int estabelecimentoId,
        CancellationToken cancellationToken)
        => Json(await apiClient.BuscarCatalogoAsync(
            descricao,
            clienteId,
            tblPrecoId,
            estabelecimentoId,
            cancellationToken));

    /// <summary>
    /// Proxy server-side para buscar o conteúdo do arquivo JSON do punchout,
    /// equivalente ao GetConteudoArquivoPedido do PHP.
    /// </summary>
    [HttpGet("PrePedido/conteudo-arquivo")]
    public async Task<IActionResult> GetConteudoArquivo(
        [FromQuery] string cdExtCliente,
        [FromQuery] string ordemCompra,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://punchout.brsupply.com.br/storage/processadorERP/{Uri.EscapeDataString(cdExtCliente)}/{Uri.EscapeDataString(ordemCompra)}.json";

            using var client = new HttpClient();
            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Json(new { success = false, conteudo = string.Empty });

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            return Json(new { success = true, conteudo = raw });
        }
        catch
        {
            return Json(new { success = false, conteudo = string.Empty });
        }
    }

    [HttpPut("PrePedido/{id:int}/cnpj")]
    public async Task<IActionResult> AtualizarCnpj(
        int id,
        [FromBody] AtualizarCnpjRequest request,
        CancellationToken cancellationToken)
        => Json(await apiClient.AtualizarCnpjAsync(id, request.Cnpj, cancellationToken));

    [HttpPut("PrePedido/{id:int}/endereco")]
    public async Task<IActionResult> AtualizarEndereco(
        int id,
        [FromBody] AtualizarEnderecoRequest request,
        CancellationToken cancellationToken)
        => Json(await apiClient.AtualizarEnderecoAsync(
            id,
            request.ClienteEnderecoID,
            request.Logradouro,
            cancellationToken));

    [HttpPut("PrePedido/{id:int}/local-entrega")]
    public async Task<IActionResult> AtualizarLocalEntrega(
        int id,
        [FromBody] AtualizarLocalEntregaRequest request,
        CancellationToken cancellationToken)
        => Json(await apiClient.AtualizarLocalEntregaAsync(
            id,
            request.ClienteLocalEntregaID,
            request.NmLocalEntrega,
            cancellationToken));

    [HttpPut("PrePedido/{id:int}/itens/{itemId:int}/quantidade")]
    public async Task<IActionResult> AtualizarQuantidade(
        int id,
        int itemId,
        [FromBody] AtualizarQuantidadeRequest request,
        CancellationToken cancellationToken)
        => Json(await apiClient.AtualizarQuantidadeAsync(
            id,
            itemId,
            request.Quantidade,
            request.Descricao,
            cancellationToken));

    [HttpPost("PrePedido/{id:int}/itens/{itemId:int}/excluir")]
    public async Task<IActionResult> ExcluirItem(
        int id,
        int itemId,
        [FromBody] ExcluirItemRequest request,
        CancellationToken cancellationToken)
        => Json(await apiClient.ExcluirItemAsync(id, itemId, request.Descricao, cancellationToken));

    [HttpPost("PrePedido/{id:int}/itens/{itemId:int}/trocar")]
    public async Task<IActionResult> TrocarItem(
        int id,
        int itemId,
        [FromBody] TrocarItemRequest request,
        CancellationToken cancellationToken)
        => Json(await apiClient.TrocarItemAsync(
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
            cancellationToken));

    [HttpPost("PrePedido/{id:int}/itens/adicionar")]
    public async Task<IActionResult> AdicionarItem(
        int id,
        [FromBody] AdicionarItemRequest request,
        CancellationToken cancellationToken)
        => Json(await apiClient.AdicionarItemAsync(
            id,
            request.CodItemBR,
            request.DescrItemBR,
            request.Quantidade,
            request.PrecoTbl,
            request.ItemDePara,
            request.ItemID,
            request.OrdemCompra,
            cancellationToken));

    [HttpPost("PrePedido/{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(
        int id,
        CancellationToken cancellationToken)
        => Json(await apiClient.CancelarAsync(id, cancellationToken));

    [HttpPost("PrePedido/{id:int}/reprocessar")]
    public async Task<IActionResult> Reprocessar(
        int id,
        CancellationToken cancellationToken)
        => Json(await apiClient.ReprocessarAsync(id, cancellationToken));

    [HttpPost("PrePedido/{id:int}/aceitar")]
    public async Task<IActionResult> AceitarPedido(
        int id,
        CancellationToken cancellationToken)
        => Json(await apiClient.AceitarPedidoAsync(id, cancellationToken));

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
