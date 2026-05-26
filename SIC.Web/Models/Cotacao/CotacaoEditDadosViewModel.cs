namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Dados para pré-popular o formulário de edição da proposta.
/// </summary>
public sealed class CotacaoEditDadosViewModel
{
    public int PropostaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoCotacao { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int? ClienteEnderecoID { get; set; }
    public int? ClienteLocalEntregaID { get; set; }
    public string? ObsLocalEntrega { get; set; }
    public int? TabelaPrecoID { get; set; }
    public string TabelaPrecoNome { get; set; } = string.Empty;
    public bool FlagPrecoConformeTabela { get; set; }
    public string UfOrigem { get; set; } = string.Empty;
    public string UfDestino { get; set; } = string.Empty;
    public int? CodigoIBGE { get; set; }
    public decimal? MargemPadrao { get; set; }
    public DateTime? DataValidade { get; set; }
    public int? CondPagtoId { get; set; }
    public int? FormaPagamentoSAP { get; set; }
    public string? TipoOVSAP { get; set; }
    public string? OrdemCompra { get; set; }
    public string? NrContrato { get; set; }
    public int? TipoMotivoIDSAP { get; set; }
    public string? ContatoNome { get; set; }
    public string? ContatoEmail { get; set; }
    public string? Obs { get; set; }
    public int StatusID { get; set; }
    public string StatusNome { get; set; } = string.Empty;
}
