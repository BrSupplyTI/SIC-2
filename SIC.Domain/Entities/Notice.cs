namespace SIC.Domain.Entities;

public sealed class Notice
{
    public int AvisoID { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataHoraEnvio { get; set; }
    public int Prioridade { get; set; }
}
