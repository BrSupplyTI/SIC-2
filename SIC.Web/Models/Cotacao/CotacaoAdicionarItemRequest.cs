namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Payload do corpo da requisição para adicionar item à cotação.
/// </summary>
public sealed record CotacaoAdicionarItemRequest(
    string CodItemBR,
    string DescrItemBR,
    string TipoCusto,
    decimal PrecoItem,
    decimal VlrCustoAquisicao,
    decimal VlrCustoMedio,
    int Quantidade,
    decimal VlrPrecoMinimo,
    decimal VlrTabelaPreco);
