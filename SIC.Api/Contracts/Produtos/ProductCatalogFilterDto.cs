namespace SIC.Api.Contracts.Produtos;

public sealed class ProductCatalogFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public string? ComecaComTexto { get; set; }
    public string? ContemTexto { get; set; }
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
    public string? OrderBy { get; set; } = "Nome (A-Z)";
}
