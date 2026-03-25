namespace SIC.Domain.Entities;

public sealed class OrderLogItem
{
    public string Origem { get; set; } = string.Empty;
    public DateTime? DataHora { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string NmUsuario { get; set; } = string.Empty;
}
