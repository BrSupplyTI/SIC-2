namespace SIC.Domain.Entities;

public sealed class LiberacaoPedidoItem
{
    public int CotacaoID { get; set; }
    public int AgrupadorFrete { get; set; }
    public decimal VlrFreteCalc { get; set; }
    public int TransportadoraID { get; set; }
    public string TipoOVSAP { get; set; } = string.Empty;
    public int QtDiasParado { get; set; }
    public DateTime? DataCotacao { get; set; }
    public DateTime? DataProgEntrega { get; set; }
    public DateTime? DataProgEmbarque { get; set; }
    public DateTime? DataProgLiberacao { get; set; }
    public DateTime? DataSLACliente { get; set; }
    public string StatusSLACliente { get; set; } = string.Empty;
    public string OrdemCompra { get; set; } = string.Empty;
    public string NmCliente { get; set; } = string.Empty;
    public int ClienteID { get; set; }
    public string RazaoSocialCliente { get; set; } = string.Empty;
    public int CarteiraID { get; set; }
    public string NmCarteira { get; set; } = string.Empty;
    public string CdControle { get; set; } = string.Empty;
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string NmCategoria { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string NmCanalVenda { get; set; } = string.Empty;
    public int QtItens { get; set; }
    public int QtRuptura { get; set; }
    public decimal ValorPedido { get; set; }
    public string LiberarAutomatico { get; set; } = string.Empty;
    public string FormaPagto { get; set; } = string.Empty;
    public decimal MargemBruta { get; set; }
    public int FlagNaoEditarPedidoComOC { get; set; }
    public int FlagNaoLiberarPedidoSemOC { get; set; }
    public string OC_Preenchida { get; set; } = string.Empty;
    public decimal VlrFrete { get; set; }
    public decimal VlrTaxaServico { get; set; }
    public string StatusIntegradoSAP { get; set; } = string.Empty;
    public string DescricaoErroSAP { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
    public string Solicitante { get; set; } = string.Empty;
    public string CdExtCliente { get; set; } = string.Empty;
    public string MsgOrdemCompraObrigatoria { get; set; } = string.Empty;
}
