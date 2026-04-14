namespace SIC.Api.Contracts.Admin;

public sealed class AdminNoticeDto
{
    public int AvisoID { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int Prioridade { get; set; }
    public DateTime DataHoraEnvio { get; set; }
    public DateTime DataHoraExpiracao { get; set; }
    public string Responsavel { get; set; } = string.Empty;
    public string Destinatario { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public int QtLeituras { get; set; }
}
