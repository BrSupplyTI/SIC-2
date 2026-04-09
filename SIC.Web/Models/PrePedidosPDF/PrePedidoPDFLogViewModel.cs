namespace SIC.Web.Models.PrePedidosPDF;

/// <summary>
/// ViewModel de log do pré-pedido (getLogs).
/// </summary>
public sealed class PrePedidoPDFLogViewModel
{
    public string Mensagem { get; set; } = string.Empty;
    public string CriadoEmFormatado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
