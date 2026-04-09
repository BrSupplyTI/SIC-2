namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Entidade de resultado da busca no catálogo (BuscarCatalogo).
/// Campos mapeados exatamente da query PHP original.
/// </summary>
public sealed class PrePedidoPDFCatalogoItem
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
    public string VlrCustoAquisicao { get; set; } = string.Empty;
    public string VlrCustoMedio { get; set; } = string.Empty;
    public decimal VlrTabela { get; set; }
    public string Criticidade { get; set; } = string.Empty;
    public string TabelaPreco { get; set; } = string.Empty;
    public string ItemDePara { get; set; } = string.Empty;
}
