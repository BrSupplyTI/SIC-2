namespace SIC.Api.Contracts.Liberacao;

/// <summary>
/// Retorno da validação de liberação de um pedido.
/// Mensagens já expandidas (pipe-separated) em listas individuais para facilitar o consumo.
/// </summary>
public sealed class LiberacaoPedidoAnaliseDto
{
    /// <summary>Pedido pronto para ser liberado/integrado (nenhuma linha com FlagErro=1).</summary>
    public bool PedidoPronto { get; set; }

    /// <summary>Mensagens de erro (cada item é uma mensagem individual).</summary>
    public List<string> Erros { get; set; } = [];

    /// <summary>Mensagens de alerta (cada item é uma mensagem individual).</summary>
    public List<string> Alertas { get; set; } = [];

    /// <summary>Mensagens informativas neutras (ex.: pedido só com Marketplace).</summary>
    public List<string> Informacoes { get; set; } = [];
}
