namespace SIC.Web.Models.Auth;

public sealed class EstablishmentVm
{
    public int EstabelecimentoId { get; set; }
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string? CdEstabelecimento { get; set; }
    public bool IsCurrent { get; set; }
}
