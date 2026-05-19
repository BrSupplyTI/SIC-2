namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Parâmetros para gravação do log de envio de e-mail de cotação
/// (INSERT em BRWeb..Proposta_CotacaoEnvio).
/// </summary>
public sealed class SalvarLogEnvioRequest
{
    public int     PropostaId            { get; set; }
    public string  Nome                  { get; set; } = string.Empty;
    public string  Email                 { get; set; } = string.Empty;
    public string  Saudacao              { get; set; } = string.Empty;
    public string? Mensagem              { get; set; }
    public string? ComCopia              { get; set; }
    public string  Hash                  { get; set; } = string.Empty;
    public int     UsuarioId             { get; set; }
    public int     PodeDispEstoque       { get; set; }
    public int     PodeAltTransportadora { get; set; }
    public int     PodeAltCondPagamento  { get; set; }
    public int     PodeNegociar          { get; set; }
}
