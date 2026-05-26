namespace SIC.Domain.Entities.Liberacao;

/// <summary>
/// Resultado da análise de liberação de um pedido (SP SIC_AnaliseLiberacaoPedido).
/// Cada linha retornada pela SP tem mensagens pipe-separated.
/// </summary>
public sealed class LiberacaoPedidoAnalise
{
    public int FlagErro { get; set; }
    public int FlagAlerta { get; set; }
    public string MensagemErro { get; set; } = string.Empty;
    public string MensagemAlerta { get; set; } = string.Empty;
}
