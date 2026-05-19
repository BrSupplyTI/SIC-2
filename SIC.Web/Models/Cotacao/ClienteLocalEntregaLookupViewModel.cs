namespace SIC.Web.Models.Cotacao;

public sealed class ClienteLocalEntregaLookupViewModel
{
    public int ClienteLocalEntregaId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Logradouro { get; set; }
    public string? CdUF { get; set; }
    public string? Cidade { get; set; }
    public int FlagEnderecoDiferente { get; set; }
    public string? CdControle { get; set; }
    public string? ObsLocalEntrega { get; set; }
    public string? TipoOVSAP { get; set; }
    public int? CondPagtoId { get; set; }
}
