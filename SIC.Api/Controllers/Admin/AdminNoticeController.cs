using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Admin;
using SIC.Api.Services.Admin;

namespace SIC.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/mensagens")]
public sealed class AdminNoticeController(IAdminNoticeService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await service.GetAllNoticesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoticeRequest request, CancellationToken cancellationToken)
    {
        await service.CreateNoticeAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{avisoId:int}/expirar")]
    public async Task<IActionResult> Expire(int avisoId, CancellationToken cancellationToken)
    {
        await service.ExpireNoticeAsync(avisoId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{avisoId:int}")]
    public async Task<IActionResult> Delete(int avisoId, CancellationToken cancellationToken)
    {
        await service.DeleteNoticeAsync(avisoId, cancellationToken);
        return NoContent();
    }

    [HttpGet("areas")]
    public async Task<IActionResult> GetAreas(CancellationToken cancellationToken)
    {
        var result = await service.GetAreasAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("usuarios")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var result = await service.GetActiveUsersAsync(cancellationToken);
        return Ok(result);
    }
}
