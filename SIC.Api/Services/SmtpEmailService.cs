using System.Net;
using System.Net.Mail;

namespace SIC.Api.Services;

public sealed class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    private readonly string _host = configuration["Smtp:Host"] ?? string.Empty;
    private readonly int _port = configuration.GetValue<int?>("Smtp:Port") ?? 587;
    private readonly bool _enableSsl = configuration.GetValue<bool?>("Smtp:EnableSsl") ?? true;
    private readonly string _username = configuration["Smtp:Username"] ?? string.Empty;
    private readonly string _password = configuration["Smtp:Password"] ?? string.Empty;
    private readonly string _fromEmail = configuration["Smtp:FromEmail"] ?? string.Empty;
    private readonly string _fromName = configuration["Smtp:FromName"] ?? "SIC";

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_host)
            || string.IsNullOrWhiteSpace(_fromEmail)
            || string.IsNullOrWhiteSpace(_username)
            || string.IsNullOrWhiteSpace(_password))
        {
            throw new InvalidOperationException("Configuração SMTP incompleta em appsettings (Smtp:*).");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        using var client = new SmtpClient(_host, _port)
        {
            EnableSsl = _enableSsl,
            Credentials = new NetworkCredential(_username, _password)
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
