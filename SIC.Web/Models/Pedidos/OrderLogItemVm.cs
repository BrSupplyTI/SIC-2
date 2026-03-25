namespace SIC.Web.Models.Pedidos;

public sealed class OrderLogItemVm
{
    public string Origem { get; set; } = string.Empty;
    public string? DataHora { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string NmUsuario { get; set; } = string.Empty;
}
