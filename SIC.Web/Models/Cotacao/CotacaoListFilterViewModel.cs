namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Filtros da listagem de cotações (enviados via querystring GET).
/// </summary>
public sealed class CotacaoListFilterViewModel
{
    public int? UsuarioID { get; set; }
    public int FiltroCotacao { get; set; } = 1;
    public string? CdExtCliente { get; set; }
    public int? PropostaId { get; set; }
    public string? CNPJ { get; set; }
    public int? EstabelecimentoID { get; set; }
    public int? StatusID { get; set; }
    public string? DataInicial { get; set; }
    public string? DataFinal { get; set; }
}
