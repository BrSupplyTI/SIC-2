namespace SIC.Web.Models.Abreviacoes;

public sealed class AbreviacaoItemViewModel
{
    public int ID { get; init; }
    public string Texto { get; init; } = string.Empty;
    public string Abreviacao { get; init; } = string.Empty;
    public string NmUsuario { get; init; } = string.Empty;
    public string DataHora { get; init; } = string.Empty;
}
