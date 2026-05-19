namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Opção genérica de select (Id + Nome).
/// </summary>
public sealed class CotacaoSelectOptionViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
