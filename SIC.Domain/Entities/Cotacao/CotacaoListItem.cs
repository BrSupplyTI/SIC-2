namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Item retornado pela listagem de cotações (SIC_ListaPropostas).
/// </summary>
public sealed class CotacaoListItem
{
    public string CdExtCliente { get; set; } = string.Empty;
    public int PropostaId { get; set; }
    public string CdProposta { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string DtCriacao { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteCNPJ { get; set; } = string.Empty;
    public decimal MargemPadrao { get; set; }
    public decimal Frete { get; set; }
    public string DataValidade { get; set; } = string.Empty;
    public string DataValidadeSQL { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Obs { get; set; } = string.Empty;
    public string NmMotivo { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty;
    public int? CotacaoID { get; set; }
    public int? CotacaoStatusID { get; set; }
    public string CotacaoStatus { get; set; } = string.Empty;
    public decimal TotalVenda { get; set; }
    public string TipoCotacao { get; set; } = string.Empty;
    public string NmCondPagto { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public int QtdItens { get; set; }
    public int? EstabelecimentoID { get; set; }
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string DataAbertura { get; set; } = string.Empty;
    public string DataAberturaSQL { get; set; } = string.Empty;
    public string Executivo { get; set; } = string.Empty;
    public string AprovadorNmUsuario { get; set; } = string.Empty;
}
