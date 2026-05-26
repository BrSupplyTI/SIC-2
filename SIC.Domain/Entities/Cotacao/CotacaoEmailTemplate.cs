namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Dados completos de uma proposta para montagem do template HTML de e-mail.
/// Equivalente à classe DadosEmail interna de CotacaoEmailService.
/// </summary>
public sealed class CotacaoEmailTemplate
{
    public string CdProposta          { get; set; } = string.Empty;
    public string OrdemCompra         { get; set; } = string.Empty;
    public string Obs                 { get; set; } = string.Empty;
    public string ContatoNome         { get; set; } = string.Empty;
    public string ContatoEmail        { get; set; } = string.Empty;
    public string DataValidade        { get; set; } = string.Empty;
    public string CondPagtoNome       { get; set; } = string.Empty;
    public string StatusNome          { get; set; } = string.Empty;
    public int    DiasPrazoEntrega    { get; set; }
    public string TransportadoraNome  { get; set; } = string.Empty;
    public decimal VlrFrete           { get; set; }
    public decimal TotalVendaSemFrete { get; set; }
    public decimal TotalVendaFinal    { get; set; }

    public string EstabRazaoSocial    { get; set; } = string.Empty;
    public string EstabCNPJ           { get; set; } = string.Empty;
    public string EstabInscrEstadual  { get; set; } = string.Empty;
    public string EstabTelefone       { get; set; } = string.Empty;
    public string EstabEndereco       { get; set; } = string.Empty;
    public string EstabNumero         { get; set; } = string.Empty;
    public string EstabComplemento    { get; set; } = string.Empty;
    public string EstabBairro         { get; set; } = string.Empty;
    public string EstabCidade         { get; set; } = string.Empty;
    public string EstabUF             { get; set; } = string.Empty;
    public string EstabCEP            { get; set; } = string.Empty;

    public string ConsultorNome       { get; set; } = string.Empty;
    public string ConsultorEmail      { get; set; } = string.Empty;
    public string ConsultorTelefone   { get; set; } = string.Empty;

    public string ClienteRazaoSocial  { get; set; } = string.Empty;
    public string ClienteCNPJ         { get; set; } = string.Empty;
    public string ClienteTelefone     { get; set; } = string.Empty;
    public string ClienteEndereco     { get; set; } = string.Empty;
    public string ClienteNumero       { get; set; } = string.Empty;
    public string ClienteComplemento  { get; set; } = string.Empty;
    public string ClienteBairro       { get; set; } = string.Empty;
    public string ClienteCidade       { get; set; } = string.Empty;
    public string ClienteUF           { get; set; } = string.Empty;
    public string ClienteCEP          { get; set; } = string.Empty;

    public IReadOnlyList<CotacaoEmailTemplateItem> Itens { get; set; } = [];
}
