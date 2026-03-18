using Microsoft.AspNetCore.Mvc;
using SIC.Api.Models.Profile;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProfileController(IUserProfileService profileService) : ControllerBase
{
    [HttpGet("areas")]
    public async Task<IActionResult> GetAreas(CancellationToken cancellationToken)
    {
        var areas = await profileService.GetAreasAsync(cancellationToken);
        return Ok(areas);
    }

    [HttpGet("{usuarioId:int}")]
    public async Task<IActionResult> Get(int usuarioId, CancellationToken cancellationToken)
    {
        var profile = await profileService.GetProfileAsync(usuarioId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await profileService.UpdateProfileAsync(request.UsuarioId, request.AreaId, request.Telefone, request.Ramal, request.Matricula, request.Cargo, request.Setor, request.DiaAniversario, request.MesAniversario, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("photo")]
    public async Task<IActionResult> UpdatePhoto([FromBody] UpdateUserPhotoRequest request, CancellationToken cancellationToken)
    {
        var result = await profileService.UpdatePhotoAsync(request.UsuarioId, request.Foto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("photo/remove")]
    public async Task<IActionResult> RemovePhoto([FromBody] RemoveUserPhotoRequest request, CancellationToken cancellationToken)
    {
        var result = await profileService.RemovePhotoAsync(request.UsuarioId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
