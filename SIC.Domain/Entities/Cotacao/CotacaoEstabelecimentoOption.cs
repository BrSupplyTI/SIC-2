namespace SIC.Domain.Entities.Cotacao;

public sealed class CotacaoEstabelecimentoOption
{
    public int EstabelecimentoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int UfId { get; set; }
}
