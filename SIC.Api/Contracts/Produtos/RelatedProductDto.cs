namespace SIC.Api.Contracts.Produtos;

public sealed class RelatedProductDto
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public string? Foto { get; set; }
}
