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
}
