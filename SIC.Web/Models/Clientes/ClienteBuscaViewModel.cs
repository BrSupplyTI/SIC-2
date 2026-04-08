using SIC.Web.Models.Produtos;

namespace SIC.Web.Models.Clientes;

public sealed class ClienteBuscaViewModel
{
    public string? Texto { get; set; }
    public string TipoBusca { get; set; } = "comeca";
    public int FlagAtivo { get; set; } = 1;
    public int EstabelecimentoID { get; set; } = 0;
    public int FlagClienteMae { get; set; } = 0;
    public int CarteiraID { get; set; } = 0;
    public int QtDiasUltimoPedido { get; set; } = 0;
    public string OrderBy { get; set; } = "Nome (A-Z)";

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ClienteSearchItemVm> Itens { get; set; } = [];
    public IReadOnlyList<CatalogEstablishmentVm> Estabelecimentos { get; set; } = [];
    public IReadOnlyList<CarteiraVm> Carteiras { get; set; } = [];
}
