using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIC.Web.Models.Cotacao;

public sealed class CotacaoAddViewModel
{
    // ── Identificação (usado na edição) ──

    public int PropostaId { get; set; }
    public int StatusID { get; set; }
    public string? StatusNome { get; set; }

    // ── Dados Principais ──

    [Display(Name = "Tipo")]
    public string? Tipo { get; set; }

    public int TipoID { get; set; } = 2;

    [Display(Name = "Nome da Cotação")]
    public string? NomeCotacao { get; set; }

    [Display(Name = "Estabelecimento")]
    public string? Estabelecimento { get; set; }

    [Display(Name = "Cliente")]
    public string? Cliente { get; set; }

    [Display(Name = "Endereço")]
    public string? Endereco { get; set; }

    [Display(Name = "Local de Entrega")]
    public string? LocalEntrega { get; set; }

    [Display(Name = "Observação do local de entrega")]
    public string? ObsLocalEntrega { get; set; }

    [Display(Name = "Tabela de Preço")]
    public string? TabelaPreco { get; set; }

    [Display(Name = "Tabela de Preço ID")]
    public int? TabelaPrecoId { get; set; }

    [Display(Name = "Calcular Conforme Tabela de Preço")]
    public bool PrecoItens { get; set; }

    // ── Tributação / Localização ──

    [Display(Name = "UF Origem")]
    public string? UfOrigem { get; set; }

    [Display(Name = "UF Destino")]
    public string? UfDestino { get; set; }

    [Display(Name = "Cidade Destino")]
    public string? CidadeDestino { get; set; }

    [Display(Name = "Margem Padrão (%)")]
    public string? MargemPadrao { get; set; } = "0,00";

    // ── Condições Comerciais ──

    [Display(Name = "Validade")]
    public string? Validade { get; set; } = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

    [Display(Name = "Condição de Pagamento")]
    public int? CondPagtoId { get; set; }

    [Display(Name = "Forma de Pagamento")]
    public int? FormaPagtoId { get; set; }

    [Display(Name = "Tipo de Ordem")]
    public string? TipoOrdem { get; set; }

    [Display(Name = "Ordem de Compra")]
    public string? OrdemCompra { get; set; }

    /// <summary>Texto do tipo selecionado, preenchido via hidden input no JS.</summary>
    public string? TipoNome { get; set; }

    [Display(Name = "Nº do Contrato")]
    public string? NrContrato { get; set; }

    [Display(Name = "Motivo")]
    public int? MotivoBonificacaoId { get; set; }

    public int? TipoMotivoIDSAP { get; set; }

    [Display(Name = "Número Chamado")]
    public string? NrChamado { get; set; }

    [Display(Name = "Pedido Original")]
    public int? PedidoOriginal { get; set; }

    // ── Contato ──

    [Display(Name = "Nome Contato (Externo)")]
    public string? NomeContatoExterno { get; set; }

    [Display(Name = "E-mail Contato (Externo)")]
    public string? EmailContatoExterno { get; set; }

    // ── Observações ──

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    // ── Lookup lists ──

    public List<SelectListItem> Tipos { get; set; } = [];
    public List<SelectListItem> Estabelecimentos { get; set; } = [];
    public List<SelectListItem> ClienteOptions { get; set; } = [];
    public List<SelectListItem> EnderecoOptions { get; set; } = [];
    public List<SelectListItem> LocalEntregaOptions { get; set; } = [];
    public List<SelectListItem> ObsLocalEntregaOptions { get; set; } = [];
    public List<SelectListItem> TabelaPrecoOptions { get; set; } = [];
    public List<SelectListItem> UfDestinoOptions { get; set; } = [];
    public List<SelectListItem> CidadeDestinoOptions { get; set; } = [];
    public List<SelectListItem> CondicoesPagamento { get; set; } = [];
    public List<SelectListItem> FormasPagamento { get; set; } = [];
    public List<SelectListItem> TipoOrdemOptions { get; set; } = [];
    public List<SelectListItem> MotivosBonificacao { get; set; } = [];

    /// <summary>
    /// Mapa EstabelecimentoID → CdUF (para preencher UF Origem via JS).
    /// </summary>
    public Dictionary<string, string> EstabelecimentoUfMap { get; set; } = [];
}
