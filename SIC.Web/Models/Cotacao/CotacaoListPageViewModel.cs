namespace SIC.Web.Models.Cotacao;

/// <summary>
/// ViewModel da página de listagem: itens + filtro + opções dos selects.
/// </summary>
public sealed class CotacaoListPageViewModel
{
    // ── Dados da grid ──
    public IReadOnlyList<CotacaoListItemViewModel> Itens { get; set; } = [];

    // ── Filtro aplicado ──
    public CotacaoListFilterViewModel Filtro { get; set; } = new();

    // ── Datas efetivas (para exibir nos badges / hidden inputs) ──
    public DateTime FiltroDataInicial { get; set; }
    public DateTime FiltroDataFinal { get; set; }
    public bool FiltroAplicado { get; set; }

    // ── Opções dos selects ──
    public IReadOnlyList<SelectOptionViewModel> EstabelecimentoOptions { get; set; } = [];
    public IReadOnlyList<SelectOptionViewModel> StatusOptions { get; set; } = [];

    // ── Paginação ──
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 25;
    public int TotalRegistros { get; set; }
    public int TotalPaginas => TotalRegistros == 0 ? 1 : (int)Math.Ceiling((double)TotalRegistros / TamanhoPagina);
}

/// <summary>
/// Item genérico para popular dropdowns (Id + Nome).
/// </summary>
public sealed class SelectOptionViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
