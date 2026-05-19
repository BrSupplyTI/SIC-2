namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Dados do detalhe completo da proposta retornado pela API.
/// </summary>
public sealed class CotacaoDetalheViewModel
{
    public int PropostaID { get; set; }
    public string CdProposta { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int Versao { get; set; }
    public string OrdemCompra { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public string StatusNome { get; set; } = string.Empty;
    public string TipoCotacao { get; set; } = string.Empty;
    public string DataValidade { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string EstabelecimentoNome { get; set; } = string.Empty;
    public string EstabelecimentoCNPJ { get; set; } = string.Empty;
    public string EstabelecimentoRazaoSocial { get; set; } = string.Empty;
    public int ClienteID { get; set; }
    public string ClienteCodigo { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteCodNome { get; set; } = string.Empty;
    public string ClienteCNPJ { get; set; } = string.Empty;
    public string ClienteContribuinte { get; set; } = string.Empty;
    public bool EhContribuinte { get; set; }
    public int ClienteEnderecoID { get; set; }
    public string ClienteEndereco { get; set; } = string.Empty;
    public string ClienteCidadeEstado { get; set; } = string.Empty;
    public int ClienteLocalEntregaID { get; set; }
    public string LocalEntregaNome { get; set; } = string.Empty;
    public string LocalEntregaEndereco { get; set; } = string.Empty;
    public string LocalEntregaCidadeEstado { get; set; } = string.Empty;
    public string LocalEntregaObservacao { get; set; } = string.Empty;
    public string CanalVenda { get; set; } = string.Empty;
    public string TipoOrdem { get; set; } = string.Empty;
    public string TipoOVSAP { get; set; } = string.Empty;
    public bool TipoOVEhRevenda { get; set; }
    public int? TipoMotivoIDSAP { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string MotivoNome { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty;
    public int? AprovadorUsuarioID { get; set; }
    public string AprovadorNome { get; set; } = string.Empty;
    public string AprovadorJustificativa { get; set; } = string.Empty;
    public int? CondPagtoID { get; set; }
    public string CondPagtoNome { get; set; } = string.Empty;
    public int? FormaPagamentoSAP { get; set; }
    public string FormaPagamentoDesc { get; set; } = string.Empty;
    public bool FlagDefCondPagTelevendas { get; set; }
    public string TabelaPrecoID { get; set; } = string.Empty;
    public string TabelaPrecoNome { get; set; } = string.Empty;
    public bool FlagPrecoConformeTabela { get; set; }
    public decimal MargemPadrao { get; set; }
    public decimal MargemBruta { get; set; }
    public decimal MargemContribuida { get; set; }
    public decimal MargemBrutaFixa { get; set; }
    public decimal MargemContribuidaFixa { get; set; }
    public string Frete { get; set; } = string.Empty;
    public decimal ValorVendaTotal { get; set; }
    public decimal VlrContribTotal { get; set; }
    public decimal ValorContribuicaoFixo { get; set; }
    public decimal ValorTotalFixo { get; set; }
    public decimal VlrPedidoMinimo { get; set; }
    public string TotalVenda { get; set; } = string.Empty;
    public string TotalVendaFrete { get; set; } = string.Empty;
    public string TotalVendaSemImposto { get; set; } = string.Empty;
    public string TotalVendaFreteSemImposto { get; set; } = string.Empty;
    public decimal TotalPeso { get; set; }
    public int QtdItens { get; set; }
    public int DiasPrazoEntrega { get; set; }
    public string DataProgEntrega { get; set; } = string.Empty;
    public string NatOperacao { get; set; } = string.Empty;
    public string UfOrigem { get; set; } = string.Empty;
    public string UfDestino { get; set; } = string.Empty;
    public string CodigoIBGE { get; set; } = string.Empty;
    public string ContatoNome { get; set; } = string.Empty;
    public string ContatoEmail { get; set; } = string.Empty;
    public int? TransportadoraID { get; set; }
    public string TransportadoraNome { get; set; } = string.Empty;
    public int? CotacaoID { get; set; }
    public int? CotacaoIdOriginal { get; set; }
    public string CotacaoStatusDesc { get; set; } = string.Empty;
    public string CotacaoEnvioComentarios { get; set; } = string.Empty;
    public bool FlagRevisarValorProdutos { get; set; }
    public bool FlagRevisarValorFrete { get; set; }
    public bool FlagRevisarPrazoPagamento { get; set; }
    public bool FlagRevisarPrazoEntrega { get; set; }
    public bool FlagRevisarAtendimento { get; set; }
    public bool FlagRevisarPermiteTrocarMarca { get; set; }
    public bool FlagRevisarPermiteTrocarUnidade { get; set; }
    public bool FlagPrecosInformados { get; set; }
    public string CotacaoEnvioIPAprovacao { get; set; } = string.Empty;
    public int? ConsultorUsuarioID { get; set; }
    public string ConsultorNome { get; set; } = string.Empty;
    public string ConsultorEmail { get; set; } = string.Empty;
    public string CarteiraNome { get; set; } = string.Empty;
    public string Observacao { get; set; } = string.Empty;
    public string Obs { get; set; } = string.Empty;
    public string StatusCredito { get; set; } = string.Empty;
    public bool FlagPrecisaAprovacao { get; set; }
    public decimal PercMargemMinPedido { get; set; }
    public decimal PercMargemMaxPedido { get; set; }
    public int AtendenteAprovadorID { get; set; }
    public string AtendenteAprovadorNome { get; set; } = string.Empty;
    public IReadOnlyList<CotacaoItemViewModel> Itens { get; set; } = [];
}
