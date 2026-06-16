namespace SIC.Domain.Entities.Categorizacao;

public sealed class CategorizacaoItem
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = "";
    public string NmItem { get; set; } = "";
    public string NmEstabelecimento { get; set; } = "";
    public string Criticidade { get; set; } = "";
    public decimal? VlrCustoAquisicao { get; set; }
    public int QtDispEstoque { get; set; }
    public string? NmTipoLista { get; set; }
    public int? PesquisaTipoListaID { get; set; }
    public int? Prioridade { get; set; }
}

public sealed class CategorizacaoItemSemCategoria
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = "";
    public string NmItem { get; set; } = "";
    public string? NmSegmento { get; set; }
}

public sealed class CategorizacaoTipoLista
{
    public int PesquisaTipoListaID { get; set; }
    public string NmTipoLista { get; set; } = "";
}
