namespace SIC.Domain.Entities;

public sealed class OrderApprovalItem
{
    public string NmUsuario { get; set; } = string.Empty;
    public string StatusAlcada { get; set; } = string.Empty;
    public int? StatusAlcadaID { get; set; }
    public DateTime? DtAprovacao { get; set; }
    public int? NrSequencia { get; set; }
    public string TipoAlcada { get; set; } = string.Empty;
}
