namespace SIC.Domain.Entities;

public sealed class ProductSimilarStock
{
    public string CdEstabelecimento { get; set; } = string.Empty;
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string Curva { get; set; } = string.Empty;
    public string Criticidade { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public int QtDisponivel { get; set; }
}
