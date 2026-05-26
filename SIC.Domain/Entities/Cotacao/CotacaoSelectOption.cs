namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Opção de select (Id/Nome) usada em filtros e dropdowns de cotação.
/// </summary>
public sealed class CotacaoSelectOption
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
