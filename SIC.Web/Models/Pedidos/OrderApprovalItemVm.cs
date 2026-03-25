namespace SIC.Web.Models.Pedidos;

public sealed class OrderApprovalItemVm
{
    public int NrSequencia { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string TipoAlcada { get; set; } = string.Empty;
    public string StatusAlcada { get; set; } = string.Empty;
    public int StatusAlcadaID { get; set; }
    public string? DtAprovacao { get; set; }
}
