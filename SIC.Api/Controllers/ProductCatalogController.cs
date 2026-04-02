using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Produtos;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public sealed class ProductCatalogController(IProductCatalogService service) : ControllerBase
{
    [HttpGet("catalogo")]
    public async Task<IActionResult> GetCatalog([FromQuery] ProductCatalogFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await service.GetCatalogAsync(filter, cancellationToken);

        string urlSemFoto = "https://www.supplymanager.com.br/fotos/semimagem.jpg";
        string pastaRede = @"\\192.168.0.10\Fotos";
        string baseUrlPublica = "https://www.supplymanager.com.br/fotos";

        foreach (var item in result.Itens)
        {
            item.Foto = urlSemFoto;

            if (!string.IsNullOrWhiteSpace(item.CdItem))
            {
                string caminhoFisico = Path.Combine(pastaRede, item.CdItem + ".jpg");
                string urlPublica = $"{baseUrlPublica}/{item.CdItem}.jpg";

                if (System.IO.File.Exists(caminhoFisico))
                {
                    item.Foto = urlPublica;
                }
            }
        }

        return Ok(result);
    }

    [HttpGet("estabelecimentos")]
    public async Task<IActionResult> GetEstablishments(CancellationToken cancellationToken)
    {
        var result = await service.GetEstablishmentsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{itemId:int}")]
    public async Task<IActionResult> GetDetail(int itemId, CancellationToken cancellationToken)
    {
        var dto = await service.GetDetailAsync(itemId, cancellationToken);
        if (dto is null) return NotFound();

        string urlSemFoto = "https://www.supplymanager.com.br/fotos/semimagem.jpg";
        string pastaRede = @"\\192.168.0.10\Fotos";
        string baseUrlPublica = "https://www.supplymanager.com.br/fotos/high";

        dto.FotoPrincipal = urlSemFoto;
        var fotos = new List<string>();

        if (!string.IsNullOrWhiteSpace(dto.CdItem))
        {
            string caminhoFisico = Path.Combine(pastaRede, dto.CdItem + ".jpg");
            if (System.IO.File.Exists(caminhoFisico))
                dto.FotoPrincipal = $"{baseUrlPublica}/{dto.CdItem}.jpg";

            for (int i = 1; i <= 9; i++)
            {
                string sufixo = $"_{i}";
                string caminho = Path.Combine(pastaRede, dto.CdItem + sufixo + ".jpg");
                if (System.IO.File.Exists(caminho))
                    fotos.Add($"{baseUrlPublica}/{dto.CdItem}{sufixo}.jpg");
            }
        }

        dto.FotosSecundarias = fotos;
        return Ok(dto);
    }

    [HttpGet("{itemId:int}/estoques")]
    public async Task<IActionResult> GetStock(int itemId, CancellationToken cancellationToken)
    {
        var result = await service.GetStockAsync(itemId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{itemId:int}/estoques/{estabelecimentoId:int}/alocacoes")]
    public async Task<IActionResult> GetStockAllocations(int itemId, int estabelecimentoId, CancellationToken cancellationToken)
    {
        var result = await service.GetStockAllocationsAsync(itemId, estabelecimentoId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{itemId:int}/ordens-compra")]
    public async Task<IActionResult> GetPurchaseOrders(int itemId, CancellationToken cancellationToken)
    {
        var result = await service.GetPurchaseOrdersAsync(itemId, cancellationToken);
        return Ok(result);
    }
}
