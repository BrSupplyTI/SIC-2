using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Configuration;

namespace SIC.Api.Controllers;

/// <summary>
/// Endpoint para retornar configurações da aplicação
/// </summary>
[ApiController]
[Route("api/configuration")]
public sealed class ConfigurationController(IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Retorna as configurações de SMTP
    /// </summary>
    [HttpGet("smtp")]
    public ActionResult<SmtpConfigDto> GetSmtpConfig()
    {
        var config = new SmtpConfigDto
        {
            Host = configuration["Smtp:Host"] ?? string.Empty,
            HostFallback = configuration["Smtp:HostFallback"] ?? string.Empty,
            Port = configuration.GetValue<int?>("Smtp:Port") ?? 587,
            EnableSsl = configuration.GetValue<bool?>("Smtp:EnableSsl") ?? true,
            Timeout = configuration.GetValue<int?>("Smtp:Timeout") ?? 30,
            Username = configuration["Smtp:Username"] ?? string.Empty,
            Password = configuration["Smtp:Password"] ?? string.Empty,
            FromEmail = configuration["Smtp:FromEmail"] ?? string.Empty,
            FromName = configuration["Smtp:FromName"] ?? string.Empty,
        };

        if (string.IsNullOrWhiteSpace(config.Host))
        {
            return NotFound(new { error = "SMTP configuration not found" });
        }

        return Ok(config);
    }
}
