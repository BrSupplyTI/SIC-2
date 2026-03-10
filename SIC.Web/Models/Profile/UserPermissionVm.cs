namespace SIC.Web.Models.Profile;

public sealed class UserPermissionVm
{
    public string Modulo { get; set; } = string.Empty;
    public string NomePermissao { get; set; } = string.Empty;
    public string? ConcedidoPor { get; set; }
    public DateTime? DataHora { get; set; }
}
