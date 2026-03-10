namespace SIC.Api.Models.Profile;

public sealed class UserPermissionDto
{
    public string Modulo { get; set; } = string.Empty;
    public string NomePermissao { get; set; } = string.Empty;
    public string? ConcedidoPor { get; set; }
    public DateTime? DataHora { get; set; }
}
