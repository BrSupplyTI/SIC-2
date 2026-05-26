namespace SIC.Domain.Entities.Liberacao;

/// <summary>
/// Item Br Supply de um pedido (BR_CotacaoItem + BR_Item + BR_PrecoEstoque + SAP).
/// Campos derivados (Situação, FlagRuptura, Follow de Compras) são calculados na query.
/// </summary>
public sealed class LiberacaoPedidoItemBrSupply
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
    /// <summary>Situação calculada: "Bloqueado" | "Não Alocado" | "Alocado" | "Atendido".</summary>
    public string SituacaoItem { get; set; } = string.Empty;
    public int OrderBy { get; set; }
    /// <summary>FlagAlocaPedido em BR_CotacaoItem (0=não alocado, 1=alocado, 2=atendido).</summary>
    public int FlagAlocaPedido { get; set; }
    public string DsFollowCompras { get; set; } = string.Empty;
    public int NaturezaOperacaoID { get; set; }
    /// <summary>Margem calculada formatada (ex.: "12,34%") ou vazia.</summary>
    public string Margem { get; set; } = string.Empty;
    public decimal? MargemCalculada { get; set; }
    public string Previsao { get; set; } = string.Empty;
    public int FlagRuptura { get; set; }
    public int QtDisponivel { get; set; }
}

/// <summary>
/// Item Marketplace do pedido (BR_CotacaoItem + BR_ItemFornecedor + BR_Fornecedor).
/// </summary>
public sealed class LiberacaoPedidoItemMarketplace
{
    public int CotacaoItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public string NmFornecedor { get; set; } = string.Empty;
    public int QtItem { get; set; }
    public decimal VlrUnit { get; set; }
    public decimal VlrTotal { get; set; }
}

/// <summary>
/// Item compatível retornado para o modal de troca.
/// Conversão fiel do retorno da SP SIC_Itens_Compativeis_Troca.
/// </summary>
public sealed class LiberacaoPedidoItemCompativel
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

/// <summary>
/// Resultado agregado para o modal de troca — lista de itens + mensagem de análise + flag de troca automática.
/// Espelha a saída de comercial_ajax_buscar_itens_compativeis.php.
/// </summary>
public sealed class LiberacaoPedidoTrocaCompativeisResultado
{
    public IReadOnlyList<LiberacaoPedidoItemCompativel> Itens { get; set; } = Array.Empty<LiberacaoPedidoItemCompativel>();
    /// <summary>BR_ClienteConfig.FlagTrocaItemAutomatica (0=não, 1=sim/oculto, 2=permite exibir switch).</summary>
    public int FlagTrocaItemAutomatica { get; set; }
    /// <summary>Mensagem retornada por SIC_Itens_Compativeis_Troca_Analise (partes separadas por '|').</summary>
    public string MensagemAnalise { get; set; } = string.Empty;
}
