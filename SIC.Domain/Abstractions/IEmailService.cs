namespace SIC.Domain.Abstractions;

public interface IEmailService
{
    /// <summary>Envio simples (mantido para compatibilidade).</summary>
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

    /// <summary>Envio completo com CC, BCC e Reply-To.</summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
