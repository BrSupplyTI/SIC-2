namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Payload do corpo da requisição para finalizar uma cotação/proposta.
/// </summary>
public sealed record CotacaoFinalizarRequest(
    int PropostaID,
    string DataValidade,
    int UsuarioID);
