namespace SIC.Api.Contracts.Cotacao;

public sealed class CotacaoEstabelecimentoOptionDto
{
    public int EstabelecimentoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int UfId { get; set; }
}
