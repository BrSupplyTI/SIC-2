namespace SIC.Api.Contracts.Cotacao;

public sealed class CotacaoDetalheItemDto
{
    public int PropostaItemID { get; set; }
    public int PropostaID { get; set; }
    public int? ProdutoID { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal EstoqueDisponivel { get; set; }
    public decimal PrecoMinimo { get; set; }
    public decimal PrecoTabelaPreco { get; set; }
    public string TipoCusto { get; set; } = string.Empty;
    public decimal VlrCustoAquisicao { get; set; }
    public decimal VlrCustoMedio { get; set; }
    public decimal CustoLiquido { get; set; }
    public decimal PrecoItem { get; set; }
    public decimal VlrPrecoVenda { get; set; }
    public decimal Margem { get; set; }
    public decimal MargemPercentual { get; set; }
    public decimal ICMS { get; set; }
    public decimal IPI { get; set; }
    public decimal ST { get; set; }
    public decimal PIS { get; set; }
    public decimal COFINS { get; set; }
    public decimal TotalImpostos { get; set; }
    public decimal TotalSemImposto { get; set; }
    public decimal TotalComImposto { get; set; }
    public decimal ValorLiqUnit { get; set; }
    public decimal ValorICMS { get; set; }
    public decimal PercIPI { get; set; }
    public decimal ValorFundoCombPobreza { get; set; }
    public decimal ValorPis { get; set; }
    public decimal ValorCOFINS { get; set; }
    public decimal ValorFCPST { get; set; }
    public decimal ValorICMSPartilhaOrigem { get; set; }
    public decimal ValorICMSPartilhaDestino { get; set; }
    public decimal MVA { get; set; }
    public string NCM { get; set; } = string.Empty;
    public string NumCA { get; set; } = string.Empty;
    public int SegmentoID { get; set; }
    public string NmSegmento { get; set; } = string.Empty;
    public string NmFamilia { get; set; } = string.Empty;
    public string NmSubFamilia { get; set; } = string.Empty;
    public string CodBarras { get; set; } = string.Empty;
    public int? NumeroLinha { get; set; }
    public int Status { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public bool Invisivel { get; set; }
    public bool FlagCustoAlterado { get; set; }
    public string Curva { get; set; } = string.Empty;
    public string Criticidade { get; set; } = string.Empty;
    public decimal PrecoBase { get; set; }
    public string NomeTabela { get; set; } = string.Empty;
}
