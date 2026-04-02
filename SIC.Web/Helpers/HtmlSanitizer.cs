using AngleSharp.Html.Parser;

namespace SIC.Web.Helpers;

/// <summary>
/// Utiliza o AngleSharp para corrigir HTML mal formatado (tags não fechadas, aninhamento incorreto, etc.)
/// evitando que conteúdo vindo do banco interfira no restante da página.
/// </summary>
public static class HtmlSanitizer
{
    private static readonly HtmlParser s_parser = new();

    /// <summary>
    /// Faz o parse do fragmento HTML e retorna o HTML com todas as tags corretamente fechadas.
    /// </summary>
    public static string CloseOpenTags(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        using var doc = s_parser.ParseDocument($"<body>{html}</body>");
        return doc.Body?.InnerHtml ?? string.Empty;
    }
}
