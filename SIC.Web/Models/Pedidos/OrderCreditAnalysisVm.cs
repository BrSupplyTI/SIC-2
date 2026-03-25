namespace SIC.Web.Models.Pedidos;

public sealed class OrderCreditAnalysisVm
{
    public string MotivoBloqueio { get; set; } = string.Empty;
    public int? FlagAprovado { get; set; }
    public string StatusAprovacao { get; set; } = string.Empty;
    public string? DataHoraBloqueio { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string? DataHoraAprovacao { get; set; }
    public string MotivoAprovacao { get; set; } = string.Empty;
}
