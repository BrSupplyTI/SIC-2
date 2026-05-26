namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Item retornado pela busca de catálogo de produtos para Cotação.
/// </summary>
public sealed class CotacaoCatalogoItem
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int SegmentoID { get; set; }
    public string NmSegmento { get; set; } = string.Empty;
    public int FamiliaID { get; set; }
    public string NmFamilia { get; set; } = string.Empty;
    public int SubFamiliaID { get; set; }
    public string NmSubFamilia { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string Curva { get; set; } = string.Empty;
    public int QtdDisponivel { get; set; }
    public int QtEstoqueSIC { get; set; }
    public string Ativo { get; set; } = string.Empty;
    public decimal VlrCustoAquisicao { get; set; }
    public decimal VlrCustoMedio { get; set; }
    public decimal VlrTabela { get; set; }
    public decimal VlrPrecoMinimo { get; set; }
    public string Criticidade { get; set; } = string.Empty;
    public string TabelaPreco { get; set; } = string.Empty;
}
