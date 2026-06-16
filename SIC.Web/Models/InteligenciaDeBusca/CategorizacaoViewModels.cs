using System.Text.Json.Serialization;

namespace SIC.Web.Models.InteligenciaDeBusca;

public sealed class CategorizacaoPageViewModel
{
    public IReadOnlyList<CategorizacaoItemViewModel>             Itens             { get; init; } = [];
    public IReadOnlyList<CategorizacaoItemSemCategoriaViewModel> ItensSemCategoria { get; init; } = [];
    public IReadOnlyList<CategoriaViewModel>                     Categorias        { get; init; } = [];
    public int TotalSemCategoria => ItensSemCategoria.Count;
}

public sealed class CategorizacaoItemViewModel
{
    [JsonPropertyName("itemID")]                  public int     ItemID                { get; set; }
    [JsonPropertyName("cdItem")]                  public string  CdItem                { get; set; } = "";
    [JsonPropertyName("nmItem")]                  public string  NmItem                { get; set; } = "";
    [JsonPropertyName("nmEstabelecimento")]        public string  NmEstabelecimento     { get; set; } = "";
    [JsonPropertyName("criticidade")]             public string  Criticidade           { get; set; } = "";
    [JsonPropertyName("vlrCustoAquisicaoFormat")] public string  VlrCustoAquisicaoFormat { get; set; } = "";
    [JsonPropertyName("qtDispEstoque")]           public int     QtDispEstoque         { get; set; }
    [JsonPropertyName("categoria")]               public string? Categoria             { get; set; }
    [JsonPropertyName("pesquisaTipoListaID")]      public int?    PesquisaTipoListaID   { get; set; }
    [JsonPropertyName("prioridade")]              public int?    Prioridade            { get; set; }
}

public sealed class CategorizacaoItemSemCategoriaViewModel
{
    [JsonPropertyName("itemID")]     public int     ItemID     { get; set; }
    [JsonPropertyName("cdItem")]     public string  CdItem     { get; set; } = "";
    [JsonPropertyName("nmItem")]     public string  NmItem     { get; set; } = "";
    [JsonPropertyName("nmSegmento")] public string? NmSegmento { get; set; }
}

public sealed class CategoriaViewModel
{
    [JsonPropertyName("pesquisaTipoListaID")] public int    PesquisaTipoListaID { get; set; }
    [JsonPropertyName("nmTipoLista")]         public string NmTipoLista         { get; set; } = "";
}

public sealed class SalvarCategoriaRequest
{
    [JsonPropertyName("itemID")]              public int ItemID              { get; set; }
    [JsonPropertyName("pesquisaTipoListaID")] public int PesquisaTipoListaID { get; set; }
}