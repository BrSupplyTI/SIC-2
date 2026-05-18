namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Representa uma linha da listagem de cotações (mapeamento direto da consulta SQL).
/// </summary>
public sealed class CotacaoListItemViewModel
{
    // ── Proposta ──
    public string CdExtCliente { get; set; } = string.Empty;
    public int PropostaId { get; set; }
    public string CdProposta { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string DtCriacao { get; set; } = string.Empty;
    public int ClienteId { get; set; }

    // ── Cliente ──
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteCNPJ { get; set; } = string.Empty;

    // ── Margem / Frete ──
    public decimal MargemPadrao { get; set; }
    public decimal Frete { get; set; }

    // ── Validade ──
    public string DataValidade { get; set; } = string.Empty;
    public string DataValidadeSQL { get; set; } = string.Empty;

    // ── Status ──
    public int StatusID { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // ── Observações / Motivo ──
    public string Obs { get; set; } = string.Empty;
    public string NmMotivo { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty;

    // ── Cotação ──
    public int? CotacaoID { get; set; }
    public int? CotacaoStatusID { get; set; }
    public string CotacaoStatus { get; set; } = string.Empty;

    // ── Totais ──
    public decimal TotalVenda { get; set; }
    public int QtdItens { get; set; }

    // ── Tipo / Condição ──
    public string TipoCotacao { get; set; } = string.Empty;
    public string NmCondPagto { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;

    // ── Estabelecimento ──
    public int? EstabelecimentoID { get; set; }
    public string NmEstabelecimento { get; set; } = string.Empty;

    // ── Datas abertura ──
    public string DataAbertura { get; set; } = string.Empty;
    public string DataAberturaSQL { get; set; } = string.Empty;

    // ── Executivo / Aprovador ──
    public string Executivo { get; set; } = string.Empty;
    public string AprovadorNmUsuario { get; set; } = string.Empty;
}
