namespace SIC.Web.Models.Liberacao;

public sealed class LiberacaoPedidoItemBrSupplyViewModel
{
    public int CotacaoItemID { get; set; }
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int QtItem { get; set; }
    public decimal VlrUnit { get; set; }
    public decimal VlrCusto { get; set; }
    public decimal VlrTotal { get; set; }
    public string OrdemCliente { get; set; } = string.Empty;
    public string Ordem { get; set; } = string.Empty;
    public string Sequencia { get; set; } = string.Empty;
    public string OrdemVenda { get; set; } = string.Empty;
    public string SituacaoItem { get; set; } = string.Empty;
    public int OrderBy { get; set; }
    public int FlagAlocaPedido { get; set; }
    public string DsFollowCompras { get; set; } = string.Empty;
    public int NaturezaOperacaoID { get; set; }
    public string Margem { get; set; } = string.Empty;
    public decimal? MargemCalculada { get; set; }
    public string Previsao { get; set; } = string.Empty;
    public int FlagRuptura { get; set; }
    public int QtDisponivel { get; set; }
}

public sealed class LiberacaoPedidoItemMarketplaceViewModel
{
    public int CotacaoItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public string NmFornecedor { get; set; } = string.Empty;
    public int QtItem { get; set; }
    public decimal VlrUnit { get; set; }
    public decimal VlrTotal { get; set; }
}

public sealed class LiberacaoPedidoItemCompativelViewModel
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public decimal VlrCusto { get; set; }
    public string NCM { get; set; } = string.Empty;
    public int QtEstoqueDisponivel { get; set; }
    public string ChaveTributacao { get; set; } = string.Empty;
    public decimal VlrTabelaPreco { get; set; }
}

public sealed class LiberacaoPedidoTrocaCompativeisResultadoViewModel
{
    public IReadOnlyList<LiberacaoPedidoItemCompativelViewModel> Itens { get; set; } = Array.Empty<LiberacaoPedidoItemCompativelViewModel>();
    public int FlagTrocaItemAutomatica { get; set; }
    public string MensagemAnalise { get; set; } = string.Empty;
}

// ---------- Input models (form post) ----------

public sealed class AlterarItemInputModel
{
    public int CotacaoID { get; set; }
    public int CotacaoItemID { get; set; }
    public int ItemIDOld { get; set; }
    public string CdItemOld { get; set; } = string.Empty;
    public string NmItemOld { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int QuantidadeOld { get; set; }
    /// <summary>Valor informado pelo usuário no formato pt-BR ("1.234,56"). Convertido no controller.</summary>
    public string Valor { get; set; } = string.Empty;
    public string ValorOld { get; set; } = string.Empty;
    public string OrdemItem { get; set; } = string.Empty;
    public string OrdemItemOld { get; set; } = string.Empty;
    public string Sequencia { get; set; } = string.Empty;
    public string SequenciaOld { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class AlterarItemComOvInputModel
{
    public int CotacaoID { get; set; }
    public int CotacaoItemID { get; set; }
    public string CdItemOld { get; set; } = string.Empty;
    public string NmItemOld { get; set; } = string.Empty;
    public string OrdemItem { get; set; } = string.Empty;
    public string OrdemItemOld { get; set; } = string.Empty;
    public string Sequencia { get; set; } = string.Empty;
    public string SequenciaOld { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class ExcluirItemInputModel
{
    public int CotacaoID { get; set; }
    public int CotacaoItemID { get; set; }
    public int ItemIDOld { get; set; }
    public string CdItemOld { get; set; } = string.Empty;
    public string NmItemOld { get; set; } = string.Empty;
    public string QuantidadeOld { get; set; } = string.Empty;
    public string ValorOld { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class TrocarItemInputModel
{
    public int CotacaoID { get; set; }
    public int CotacaoItemID { get; set; }
    public int ItemIDOld { get; set; }
    public string CdItemOld { get; set; } = string.Empty;
    public string NmItemOld { get; set; } = string.Empty;
    public int ItemSubstitutoID { get; set; }
    public bool FlagTrocaAutomatica { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
