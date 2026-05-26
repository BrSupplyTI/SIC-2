namespace SIC.Web.Models.Propostas;

public sealed class PropostasListViewModel
{
    public string? FiltroCodigo { get; set; }
    public string? FiltroNome { get; set; }
    public string? FiltroEstabelecimento { get; set; }
    public string? FiltroStatus { get; set; }
    public bool FiltroAplicado => !string.IsNullOrWhiteSpace(FiltroCodigo)
                               || !string.IsNullOrWhiteSpace(FiltroNome)
                               || !string.IsNullOrWhiteSpace(FiltroEstabelecimento)
                               || !string.IsNullOrWhiteSpace(FiltroStatus);

    public IReadOnlyList<PropostaItemVm> Propostas { get; set; } = [];
}
