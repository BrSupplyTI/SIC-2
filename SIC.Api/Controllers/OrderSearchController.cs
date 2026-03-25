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

    [HttpGet("{pedido:int}/integracao-sap")]
    public async Task<IActionResult> GetOrderSapIntegration(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderSapIntegrationAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/impostos")]
    public async Task<IActionResult> GetOrderTaxes(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderTaxesAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/historico-frete")]
    public async Task<IActionResult> GetFreightCalculationHistory(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetFreightCalculationHistoryAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/calculo-frete")]
    public async Task<IActionResult> GetFreightCalculation(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetFreightCalculationAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/itens-br-supply")]
    public async Task<IActionResult> GetOrderBrSupplyItems(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderBrSupplyItemsAsync(pedido, cancellationToken);

        if (result != null)
        {
            string urlSemFoto = "https://www.supplymanager.com.br/fotos/semimagem.jpg";
            string pastaRede = @"\\192.168.0.10\Fotos";
            string baseUrlPublica = "https://www.supplymanager.com.br/fotos";

            foreach (var item in result)
            {
                item.Foto = urlSemFoto;

                if (!string.IsNullOrWhiteSpace(item.CdItem))
                {
                    string caminhoFisico = Path.Combine(pastaRede, item.CdItem + ".jpg");
                    string urlPublica = $"{baseUrlPublica}/{item.CdItem + ".jpg"}";

                    if (System.IO.File.Exists(caminhoFisico))
                    {
                        item.Foto = urlPublica;
                    } else
                    {
                        item.Foto = urlSemFoto;
                    }
                }
            }
        }

        return Ok(result);
    }

    [HttpGet("{pedido:int}/itens-br-supply-ruptura")]
    public async Task<IActionResult> GetOrderBrSupplyItemsRuptura(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderBrSupplyItemsRupturaAsync(pedido, cancellationToken);

        if (result != null)
        {
            string urlSemFoto = "https://www.supplymanager.com.br/fotos/semimagem.jpg";
            string pastaRede = @"\\192.168.0.10\Fotos";
            string baseUrlPublica = "https://www.supplymanager.com.br/fotos";

            foreach (var item in result)
            {
                item.Foto = urlSemFoto;

                if (!string.IsNullOrWhiteSpace(item.CdItem))
                {
                    string caminhoFisico = Path.Combine(pastaRede, item.CdItem + ".jpg");
                    string urlPublica = $"{baseUrlPublica}/{item.CdItem + ".jpg"}";

                    if (System.IO.File.Exists(caminhoFisico))
                    {
                        item.Foto = urlPublica;
                    }
                    else
                    {
                        item.Foto = urlSemFoto;
                    }
                }
            }
        }

        return Ok(result);
    }

    [HttpGet("{pedido:int}/itens-marketplace")]
    public async Task<IActionResult> GetOrderMarketplaceItems(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderMarketplaceItemsAsync(pedido, cancellationToken);

        if (result != null)
        {
            string urlSemFoto = "https://www.supplymanager.com.br/fotos/semimagem.jpg";            
            string baseUrlPublica = "https://www.supplymanager.com.br/content/meusprodutos";

            foreach (var item in result)
            {
                item.Foto = urlSemFoto;

                if (!string.IsNullOrWhiteSpace(item.PathFoto))
                {                    
                    string urlPublica = $"{baseUrlPublica}/{item.ClienteID}/{item.PathFoto}";
                    item.Foto = urlPublica;                    
                }
            }
        }

        return Ok(result);
    }

    [HttpGet("{pedido:int}/logs-aprovacao")]
    public async Task<IActionResult> GetOrderApprovalItems(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderApprovalItemsAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/notas-fiscais")]
    public async Task<IActionResult> GetOrderInvoiceItems(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderInvoiceItemsAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/romaneios")]
    public async Task<IActionResult> GetOrderRomaneios(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderRomaneiosAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/tracking")]
    public async Task<IActionResult> GetOrderTracking(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderTrackingAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("volumes-coleta")]
    public async Task<IActionResult> GetVolumesColeta([FromQuery] string pedCli, CancellationToken cancellationToken)
    {
        var result = await service.GetVolumesColetaAsync(pedCli, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/chamados")]
    public async Task<IActionResult> GetOrderTickets(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderTicketsAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/analise-credito")]
    public async Task<IActionResult> GetOrderCreditAnalysis(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderCreditAnalysisAsync(pedido, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{pedido:int}/validacoes")]
    public async Task<IActionResult> GetOrderValidations(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderValidationsAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pedido:int}/registros-logs")]
    public async Task<IActionResult> GetOrderLogs(int pedido, CancellationToken cancellationToken)
    {
        var result = await service.GetOrderLogsAsync(pedido, cancellationToken);
        return Ok(result);
    }

    [HttpGet("nf-xml/{chave}")]
    public async Task<IActionResult> GetInvoiceXml(string chave, CancellationToken cancellationToken)
    {
        var xml = await service.GetInvoiceXmlAsync(chave, cancellationToken);
        if (xml is null) return NotFound();
        return File(System.Text.Encoding.UTF8.GetBytes(xml), "application/xml", $"{chave}.xml");
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
