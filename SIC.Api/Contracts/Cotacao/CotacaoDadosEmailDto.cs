namespace SIC.Api.Contracts.Cotacao;

public sealed class CotacaoDadosEmailDto
{
    public int PropostaId { get; set; }
    public int CotacaoID { get; set; }
    public int EstabelecimentoID { get; set; }
    public int ClienteId { get; set; }
    public string CdProposta { get; set; } = string.Empty;
    public string EstabelecimentoNome { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteCidadeEstado { get; set; } = string.Empty;
    public string ContatoNome { get; set; } = string.Empty;
    public string ContatoEmail { get; set; } = string.Empty;
    public string ConsultorNome { get; set; } = string.Empty;
    public string ConsultorEmail { get; set; } = string.Empty;
    public string ExecutivoNome { get; set; } = string.Empty;
    public string ExecutivoEmail { get; set; } = string.Empty;
    public string TotalVenda { get; set; } = string.Empty;
    public string Frete { get; set; } = string.Empty;
}
