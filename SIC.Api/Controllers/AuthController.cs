using Microsoft.AspNetCore.Mvc;
using SIC.Api.Models.Auth;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ISicAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var remoteIp = request.RemoteIp ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var result = await authService.LoginWithPasswordAsync(request.Login, request.Password, remoteIp, userAgent, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("sso-login")]
    public async Task<IActionResult> SsoLogin([FromBody] SsoLoginRequest request, CancellationToken cancellationToken)
    {
        var remoteIp = request.RemoteIp ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var result = await authService.LoginWithSsoAsync(request.Email, remoteIp, userAgent, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("validate-session")]
    public async Task<IActionResult> ValidateSession([FromBody] ValidateSessionRequest request, CancellationToken cancellationToken)
    {
        var remoteIp = request.RemoteIp ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = string.IsNullOrWhiteSpace(request.UserAgent)
            ? HttpContext.Request.Headers.UserAgent.ToString()
            : request.UserAgent;

        var result = await authService.ValidateSessionAsync(request.UsuarioId, request.SessionToken, remoteIp, userAgent, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout-session")]
    public async Task<IActionResult> LogoutSession([FromBody] LogoutSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LogoutSessionAsync(request.UsuarioId, request.SessionToken, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.RequestPasswordResetAsync(request.Identifier, remoteIp, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await authService.ResetPasswordAsync(request.Token, request.NewPassword, remoteIp, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("establishments")]
    public async Task<IActionResult> GetEstablishments([FromBody] EstablishmentListRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.GetAuthorizedEstablishmentsAsync(
            request.UsuarioId,
            request.IsAdmin,
            request.CurrentEstabelecimentoId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("change-establishment")]
    public async Task<IActionResult> ChangeEstablishment([FromBody] ChangeEstablishmentRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ChangeEstablishmentAsync(request.UsuarioId, request.IsAdmin, request.EstabelecimentoId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("update-photo")]
    public async Task<IActionResult> UpdatePhoto([FromBody] UpdateUserPhotoRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.UpdateUserPhotoAsync(request.UsuarioId, request.Foto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ChangePasswordAsync(request.UsuarioId, request.NewPassword, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private IActionResult ToActionResult(AuthResult result)
    {
        if (result.Success)
        {
            return Ok(result);
        }

        return result.ErrorCode switch
        {
            "SESSION_LOCKED" => Conflict(result),
            "INVALID_CREDENTIALS" => Unauthorized(result),
            _ => BadRequest(result)
        };
    }
}
