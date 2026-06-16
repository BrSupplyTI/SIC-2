namespace SIC.Api.Contracts.Categorizacao;

public sealed record CategorizacaoItemDto(
    int ItemID,
    string CdItem,
    string NmItem,
    string NmEstabelecimento,
    string Criticidade,
    string VlrCustoAquisicaoFormat,
    int QtDispEstoque,
    string? Categoria,
    int? PesquisaTipoListaID,
    int? Prioridade);

public sealed record CategorizacaoItemSemCategoriaDto(
    int ItemID,
    string CdItem,
    string NmItem,
    string? NmSegmento);

public sealed record CategorizacaoTipoListaDto(
    int PesquisaTipoListaID,
    string NmTipoLista);

public sealed record SalvarCategoriaRequest(int ItemID, int PesquisaTipoListaID);
