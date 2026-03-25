namespace SIC.Domain.Entities;

public sealed class OrderTicketItem
{
    public int Protocolo { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string OrigemValor { get; set; } = string.Empty;
    public string NmSolicitante { get; set; } = string.Empty;
    public string EmailSolicitante { get; set; } = string.Empty;
    public string NmArea { get; set; } = string.Empty;
    public string NmNivel { get; set; } = string.Empty;
    public string NmProblema { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public string Atraso { get; set; } = string.Empty;
    public DateTime? DtHrAbertura { get; set; }
    public DateTime? DtHrEncerramento { get; set; }
    public DateTime? PrazoResolucao { get; set; }
}
