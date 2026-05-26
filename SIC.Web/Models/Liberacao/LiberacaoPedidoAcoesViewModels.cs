namespace SIC.Web.Models.Liberacao;

// ---------- Combos e listagens read-only ----------

public sealed class LiberacaoPedidoComboItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public sealed class LiberacaoPedidoFreteOpcaoViewModel
{
    public string NomeTransportadora { get; set; } = string.Empty;
    public decimal ValorFrete { get; set; }
    public int PrazoLogistico { get; set; }
    public int PrazoComercial { get; set; }
    public decimal TaxaExtra { get; set; }
    public int QtItensRestritos { get; set; }
    public int FlagClienteFixo { get; set; }
    public int FlagObrigatoriaCanalVenda { get; set; }
    public int FlagClienteRestrito { get; set; }
}

public sealed class LiberacaoPedidoImpostoItemViewModel
{
    public string ItemDocumentoSAP { get; set; } = string.Empty;
    public string CdItem { get; set; } = string.Empty;
    public string NmItemAbrev { get; set; } = string.Empty;
    public int QtItem { get; set; }
    public decimal VlrUnitario { get; set; }
    public decimal MKUP { get; set; }
    public decimal MargemCalculada { get; set; }
    public decimal PercentualICMS { get; set; }
    public decimal ValorICMS { get; set; }
    public decimal PercentualIPI { get; set; }
    public decimal ValorIPI { get; set; }
    public decimal PercentualPIS { get; set; }
    public decimal ValorPIS { get; set; }
    public decimal PercentualCOFINS { get; set; }
    public decimal ValorCOFINS { get; set; }
    public decimal PercentualFCP { get; set; }
    public decimal ValorFundoCombPobreza { get; set; }
    public decimal ValorST { get; set; }
    public decimal ValorISS { get; set; }
    public decimal ValorICMSPartilhaOrigem { get; set; }
    public decimal ValorICMSPartilhaDestino { get; set; }
    public decimal LB { get; set; }
    public decimal ROL { get; set; }
}

// ---------- Resultado genérico ----------

public sealed class LiberacaoPedidoAcaoResultadoViewModel
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int? NovoCotacaoId { get; set; }
}
