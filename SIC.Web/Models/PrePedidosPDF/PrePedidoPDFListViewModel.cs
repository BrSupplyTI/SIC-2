namespace SIC.Web.Models.PrePedidosPDF;

public sealed class PrePedidoPDFListViewModel
{
    public int? FiltroStatus { get; set; } = 1;
    public string? FiltroCdExtCliente { get; set; }
    public DateTime? FiltroDataInicial { get; set; } = DateTime.Now.AddMonths(-1).Date;
    public DateTime? FiltroDataFinal { get; set; } = DateTime.Now.Date;
    public bool FiltroAplicado { get; set; }
    public string StatusFormatado { get; set; } = "Aguardando";
    public IReadOnlyList<PrePedidoPDFListItemViewModel> Dados { get; set; } = [];
    public IReadOnlyList<StatusPrePedidoPDFViewModel> StatusOptions { get; set; } = [];
}
