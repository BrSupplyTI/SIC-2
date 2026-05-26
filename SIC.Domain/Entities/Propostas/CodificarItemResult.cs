namespace SIC.Domain.Entities.Propostas;

public sealed class CodificarItemResult
{
    public int PropostaItemID { get; set; }
    public bool Codificado { get; set; }
    public bool SemCorrespondencia { get; set; }
    public int? ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public string Qualidade { get; set; } = string.Empty;
}
