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

        if (!string.IsNullOrWhiteSpace(dto.CdItem))
        {
            string pastaFichaTec = @"\\192.168.0.10\brs\fichatec";
            string pastaFichaSeg = @"\\192.168.0.10\brs\fispq";
            dto.HasFichaTecnica = System.IO.File.Exists(Path.Combine(pastaFichaTec, dto.CdItem + ".pdf"));
            dto.HasFichaSeguranca = System.IO.File.Exists(Path.Combine(pastaFichaSeg, dto.CdItem + ".pdf"));
        }

        return Ok(dto);
    }

    [HttpGet("{itemId:int}/ficha-tecnica")]
    public async Task<IActionResult> DownloadFichaTecnica(int itemId, CancellationToken cancellationToken)
    {
        var dto = await service.GetDetailAsync(itemId, cancellationToken);
        if (dto is null || string.IsNullOrWhiteSpace(dto.CdItem)) return NotFound();

        string caminho = Path.Combine(@"\\192.168.0.10\brs\fichatec", dto.CdItem + ".pdf");
        if (!System.IO.File.Exists(caminho)) return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(caminho, cancellationToken);
        return File(bytes, "application/pdf", $"{dto.CdItem}_FichaTecnica.pdf");
    }

    [HttpGet("{itemId:int}/ficha-seguranca")]
    public async Task<IActionResult> DownloadFichaSeguranca(int itemId, CancellationToken cancellationToken)
    {
        var dto = await service.GetDetailAsync(itemId, cancellationToken);
        if (dto is null || string.IsNullOrWhiteSpace(dto.CdItem)) return NotFound();

        string caminho = Path.Combine(@"\\192.168.0.10\brs\fispq", dto.CdItem + ".pdf");
        if (!System.IO.File.Exists(caminho)) return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(caminho, cancellationToken);
        return File(bytes, "application/pdf", $"{dto.CdItem}_FichaSeguranca.pdf");
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

    [HttpGet("{itemId:int}/similares")]
    public async Task<IActionResult> GetSimilars(int itemId, CancellationToken cancellationToken)
    {
        var result = await service.GetSimilarsAsync(itemId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{itemId:int}/similares/{itemSimilarId:int}/estoques")]
    public async Task<IActionResult> GetSimilarStock(int itemId, int itemSimilarId, CancellationToken cancellationToken)
    {
        var result = await service.GetSimilarStockAsync(itemSimilarId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{itemId:int}/relacionados")]
    public async Task<IActionResult> GetRelatedProducts(int itemId, CancellationToken cancellationToken)
    {
        var result = await service.GetRelatedProductsAsync(itemId, cancellationToken);
        return Ok(result);
    }
}
