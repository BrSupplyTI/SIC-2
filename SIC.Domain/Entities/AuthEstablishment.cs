namespace SIC.Domain.Entities;

public sealed class AuthEstablishment
{
    public int EstabelecimentoId { get; set; }
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string? CdEstabelecimento { get; set; }
}
