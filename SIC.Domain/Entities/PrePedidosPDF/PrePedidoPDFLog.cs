namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Entidade de log do pré-pedido (getLogs / getLogsErro).
/// </summary>
public sealed class PrePedidoPDFLog
{
    public string Mensagem { get; set; } = string.Empty;
    public string CriadoEmFormatado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
