namespace SIC.Web.Models.Liberacao;

public sealed class LiberacaoPedidoListViewModel
{
    // Filtros
    public string? FiltroPalavra1 { get; set; }
    public string? FiltroPalavra2 { get; set; }
    public string? FiltroPalavra3 { get; set; }
    public int FiltroOrdemCompra { get; set; }
    public int FiltroRuptura { get; set; }
    public int FiltroFrete { get; set; }
    public int FiltroMargemNegativa { get; set; }
    public decimal FiltroValorAbaixo { get; set; }
    public decimal FiltroValorAcima { get; set; }
    public string? FiltroIntegracaoSAP { get; set; }
    public string? FiltroContemItem { get; set; }
    public int FiltroAtrasados { get; set; }
    public int FiltroFretePagar { get; set; }

    // Dados
    public IReadOnlyList<LiberacaoPedidoItemViewModel> Pedidos { get; set; } = [];

    // Dashboard
    public int TotalComOV { get; set; }
    public int TotalComRuptura { get; set; }
    public int TotalAtrasados { get; set; }
    public int TotalErroOV { get; set; }
    public int TotalSemOC { get; set; }

    // Filtros ativos
    public List<FiltroAtivo> FiltrosAtivos { get; set; } = [];
}

public sealed record FiltroAtivo(string Label, string Campo);
