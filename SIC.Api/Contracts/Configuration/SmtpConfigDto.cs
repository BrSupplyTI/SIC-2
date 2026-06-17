namespace SIC.Api.Contracts.Configuration;

/// <summary>
/// DTO com configurações de SMTP retornado pela API
/// </summary>
public sealed class SmtpConfigDto
{
    public string Host { get; set; } = string.Empty;
    public string HostFallback { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public int Timeout { get; set; } = 30;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
