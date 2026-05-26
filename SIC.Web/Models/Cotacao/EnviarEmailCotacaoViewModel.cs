using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SIC.Web.Models.Cotacao;

/// <summary>
/// ViewModel da tela EnviarEmailCotacao.
/// Agrega os dados da Proposta (consulta principal), o histórico de envios
/// e os campos do formulário de envio de e-mail.
/// </summary>
public sealed class EnviarEmailCotacaoViewModel
{
    // ══════════ DADOS DA PROPOSTA (consulta principal) ══════════

    public int PropostaId { get; set; }

    /// <summary>CotacaoID do BR_Cotacao — usado no log de envio e no hash.</summary>
    public int CotacaoID { get; set; }

    /// <summary>EstabelecimentoID — usado para definir cor/logo no template.</summary>
    public int EstabelecimentoID { get; set; }

    /// <summary>Identificador interno do cliente (CdCliente)</summary>
    [ValidateNever]
    public int ClienteId { get; set; }

    /// <summary>Cotacao__CdProposta</summary>
    [ValidateNever]
    public string CdProposta { get; set; } = string.Empty;

    /// <summary>Estabelecimento__NmEstabelecimento</summary>
    [ValidateNever]
    public string EstabelecimentoNome { get; set; } = string.Empty;

    /// <summary>
    /// Nome resumido do cliente — CdExtCliente + NmCliente
    /// Fonte: consulta auxiliar de dados resumidos do cliente
    /// </summary>
    [ValidateNever]
    public string ClienteNome { get; set; } = string.Empty;

    /// <summary>
    /// Cidade e UF do cliente — ex.: "São Paulo - SP"
    /// Fonte: ClienteEndereco.Cidade + UF.CdUF (consulta auxiliar)
    /// </summary>
    [ValidateNever]
    public string ClienteCidadeEstado { get; set; } = string.Empty;

    /// <summary>Cotacao__ContatoNome</summary>
    [ValidateNever]
    public string ContatoNome { get; set; } = string.Empty;

    /// <summary>Cotacao__ContatoEmail</summary>
    [ValidateNever]
    public string ContatoEmail { get; set; } = string.Empty;

    /// <summary>Consultor__NmUsuario</summary>
    [ValidateNever]
    public string ConsultorNome { get; set; } = string.Empty;

    /// <summary>Consultor__Email — usado no checkbox BCC Atendente</summary>
    public string ConsultorEmail { get; set; } = string.Empty;

    /// <summary>Executivo__NmUsuario</summary>
    [ValidateNever]
    public string ExecutivoNome { get; set; } = string.Empty;

    /// <summary>Executivo__Email — usado no checkbox BCC Executivo</summary>
    public string ExecutivoEmail { get; set; } = string.Empty;

    /// <summary>Cotacao__TotalVenda (formatado)</summary>
    [ValidateNever]
    public string TotalVenda { get; set; } = string.Empty;

    /// <summary>Cotacao__VlrFrete (formatado)</summary>
    [ValidateNever]
    public string Frete { get; set; } = string.Empty;

    // ══════════ CAMPOS DO FORMULÁRIO DE ENVIO ══════════

    /// <summary>Texto de saudação que abre o corpo do e-mail. Ex.: "Prezado João".</summary>
    [Required(ErrorMessage = "Informe a saudação.")]
    public string Saudacao { get; set; } = string.Empty;

    /// <summary>E-mail principal do destinatário (campo Para).</summary>
    [Required(ErrorMessage = "Informe o e-mail do destinatário.")]
    [EmailAddress(ErrorMessage = "E-mail do destinatário inválido.")]
    public string EmailDestinatario { get; set; } = string.Empty;

    /// <summary>E-mail em cópia (CC). Opcional.</summary>
    [EmailAddress(ErrorMessage = "E-mail de cópia inválido.")]
    public string? ComCopia { get; set; }

    /// <summary>Mensagem personalizada exibida no corpo do e-mail.</summary>
    public string? Mensagem { get; set; }

    // ── Permissões que o cliente recebe no link da cotação ────────────────

    /// <summary>Permite que o cliente consulte disponibilidade de estoque.</summary>
    public bool PodeDispEstoque { get; set; }

    /// <summary>Permite que o cliente altere a transportadora.</summary>
    public bool PodeAltTransportadora { get; set; }

    /// <summary>Permite que o cliente altere a condição de pagamento.</summary>
    public bool PodeAltCondPagamento { get; set; }

    /// <summary>Permite que o cliente negocie valores.</summary>
    public bool PodeNegociar { get; set; }

    // ── Cópia oculta (BCC) ────────────────────────────────────────────────

    /// <summary>Quando marcado, o consultor recebe cópia oculta e é definido como Reply-To.</summary>
    public bool ConsultorRecebeCCO { get; set; }

    /// <summary>Quando marcado, o executivo recebe cópia oculta.</summary>
    public bool ExecutivoRecebeCCO { get; set; }

    // ══════════ HISTÓRICO DE ENVIOS (consulta auxiliar Proposta_CotacaoEnvio) ══════════

    [ValidateNever]
    public IReadOnlyList<CotacaoEnvioHistoricoItemViewModel> HistoricoEnvios { get; set; } = [];
}
