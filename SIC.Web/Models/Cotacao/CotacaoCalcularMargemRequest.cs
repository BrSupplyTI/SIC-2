namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Payload do corpo da requisição para calcular margem de um item da cotação.
/// </summary>
public sealed record CotacaoCalcularMargemRequest(string Type, string ViaTela);
