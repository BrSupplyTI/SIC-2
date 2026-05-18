namespace SIC.Domain.Abstractions;

public sealed class EmailMessage
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }

    public string? Cc { get; init; }
    public List<string> Bcc { get; init; } = [];
    public string? ReplyTo { get; init; }
}
