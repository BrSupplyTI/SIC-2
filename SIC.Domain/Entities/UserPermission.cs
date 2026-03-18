namespace SIC.Domain.Entities;

public sealed class UserPermission
{
    public string Modulo { get; set; } = string.Empty;
    public string NomePermissao { get; set; } = string.Empty;
    public string? ConcedidoPor { get; set; }
    public DateTime? DataHora { get; set; }
}
