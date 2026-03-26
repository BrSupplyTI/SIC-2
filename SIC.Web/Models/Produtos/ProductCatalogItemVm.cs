namespace SIC.Web.Models.Produtos;

public sealed class ProductCatalogItemVm
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
    public string? DtCadastro { get; set; }
    public string Foto { get; set; } = string.Empty;
}
