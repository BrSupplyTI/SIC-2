namespace SIC.Domain.Entities;

public sealed class OrderCreditAnalysis
{
    public string MotivoBloqueio { get; set; } = string.Empty;
    public int? FlagAprovado { get; set; }
    public string StatusAprovacao { get; set; } = string.Empty;
    public DateTime? DataHoraBloqueio { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public DateTime? DataHoraAprovacao { get; set; }
    public string MotivoAprovacao { get; set; } = string.Empty;
}
