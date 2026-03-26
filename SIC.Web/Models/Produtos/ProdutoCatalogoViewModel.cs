namespace SIC.Web.Models.Produtos;

public sealed class ProdutoCatalogoViewModel
{
    public string? Texto { get; set; }
    public string TipoBusca { get; set; } = "comeca";
    public int FlagAtivo { get; set; } = 1;
    public int FlagMarcaPropria { get; set; }
    public int EstabelecimentoID { get; set; } = 0;
    public int FlagOutlet { get; set; } = 2;
    public int FlagSobDemanda { get; set; } = 2;
    public int FlagSustentavel { get; set; }
    public int FlagNovidade { get; set; }
    public string? Curva { get; set; }
    public int FlagPadraoBrSupply { get; set; } = 1;
    public int FlagComEstoque { get; set; }
    public string OrderBy { get; set; } = "Nome (A-Z)";

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ProductCatalogItemVm> Itens { get; set; } = [];
    public IReadOnlyList<CatalogEstablishmentVm> Estabelecimentos { get; set; } = [];
}
