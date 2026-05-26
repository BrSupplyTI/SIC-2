namespace SIC.Web.Models.Liberacao;

/// <summary>
/// Resultado da análise de liberação (tela de detalhes — Comercial).
/// Mensagens já expandidas em listas individuais.
/// </summary>
public sealed class LiberacaoPedidoAnaliseViewModel
{
    public bool PedidoPronto { get; set; }
    public List<string> Erros { get; set; } = [];
    public List<string> Alertas { get; set; } = [];
    public List<string> Informacoes { get; set; } = [];

    public bool TemErros => Erros.Count > 0;
    public bool TemAlertas => Alertas.Count > 0;
    public bool TemInformacoes => Informacoes.Count > 0;
}
