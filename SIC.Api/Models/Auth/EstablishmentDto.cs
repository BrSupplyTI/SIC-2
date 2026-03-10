namespace SIC.Api.Models.Auth;

public sealed class EstablishmentDto
{
    public int EstabelecimentoId { get; set; }
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string? CdEstabelecimento { get; set; }
    public bool IsCurrent { get; set; }
}
