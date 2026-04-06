namespace SIC.Domain.Entities;

public sealed class ProductSimilar
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public DateTime DataHoraCadastro { get; set; }
    public string NCM { get; set; } = string.Empty;
}
