using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Clientes;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientController(IClientService service) : ControllerBase
{
    [HttpGet("busca")]
    public async Task<IActionResult> Search([FromQuery] ClientSearchFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await service.SearchAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{clienteId:int}")]
    public async Task<IActionResult> GetDetail(int clienteId, CancellationToken cancellationToken)
    {
        var dto = await service.GetDetailAsync(clienteId, cancellationToken);
        if (dto is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.LogoCliente))
        {
            var caminhoFisico = Path.Combine(@"\\192.168.0.10\brs\images\logotipo", dto.ClienteID.ToString(), dto.LogoCliente);
            if (System.IO.File.Exists(caminhoFisico))
            {
                dto.LogoUrl = $"https://www.supplymanager.com.br/content/images/logotipo/{dto.ClienteID}/{dto.LogoCliente}";
            }
        }

        return Ok(dto);
    }

    [HttpGet("carteiras")]
    public async Task<IActionResult> GetWallets(CancellationToken cancellationToken)
    {
        var result = await service.GetWalletsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("estabelecimentos")]
    public async Task<IActionResult> GetEstablishments(CancellationToken cancellationToken)
    {
        var result = await service.GetEstablishmentsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{clienteId:int}/consultores")]
    public async Task<IActionResult> GetConsultants(int clienteId, CancellationToken cancellationToken)
    {
        var items = await service.GetConsultantsAsync(clienteId, cancellationToken);

        foreach (var c in items)
        {
            var caminhoFoto = Path.Combine(@"\\192.168.0.10\brs\images\upload", $"{c.UsuarioID}.jpg");
            if (System.IO.File.Exists(caminhoFoto))
            {
                c.FotoUrl = $"https://www.supplymanager.com.br/content/images/upload/{c.UsuarioID}.jpg";
            }
        }

        return Ok(items);
    }

    [HttpGet("{clienteId:int}/titulos")]
    public async Task<IActionResult> GetTitulos(int clienteId, CancellationToken cancellationToken)
    {
        var items = await service.GetTitulosAsync(clienteId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{clienteId:int}/saldo-credito")]
    public async Task<IActionResult> GetCreditBalance(int clienteId, CancellationToken cancellationToken)
    {
        var dto = await service.GetCreditBalanceAsync(clienteId, cancellationToken);
        return Ok(dto);
    }

    [HttpGet("{clienteId:int}/enderecos")]
    public async Task<IActionResult> GetAddresses(int clienteId, CancellationToken cancellationToken)
    {
        var items = await service.GetAddressesAsync(clienteId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{clienteId:int}/locais-entrega")]
    public async Task<IActionResult> GetDeliveryLocations(int clienteId, CancellationToken cancellationToken)
    {
        var items = await service.GetDeliveryLocationsAsync(clienteId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{clienteId:int}/usuarios")]
    public async Task<IActionResult> GetUsers(int clienteId, CancellationToken cancellationToken)
    {
        var items = await service.GetUsersAsync(clienteId, cancellationToken);
        return Ok(items);
    }
}
