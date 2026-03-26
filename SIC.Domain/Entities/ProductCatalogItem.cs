namespace SIC.Domain.Entities;

public sealed class ProductCatalogItem
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public string NmSegmento { get; set; } = string.Empty;
    public string NmFamilia { get; set; } = string.Empty;
    public string NmSubFamilia { get; set; } = string.Empty;
    public string NmMarca { get; set; } = string.Empty;
    public string FlagTipoMarca { get; set; } = string.Empty;
    public string? NumCA { get; set; }
    public int QtEstoque { get; set; }
    public string Curva { get; set; } = string.Empty;
    public DateTime? DtCadastro { get; set; }
    public int TotalRegistros { get; set; }
}
